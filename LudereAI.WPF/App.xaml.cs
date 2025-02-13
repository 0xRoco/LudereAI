using System.IO;
using System.Windows;
using System.Windows.Threading;
using LudereAI.WPF.Interfaces;
using LudereAI.WPF.Services;
using LudereAI.WPF.ViewModels;
using LudereAI.WPF.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace LudereAI.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public IHost? Host;

    private const string DevApiUrl = "https://localhost:9099/";
    private const string StagApiUrl = "https://api-staging.ludereai.com/";
    private const string ProdApiUrl = "https://api.ludereai.com/";
    private readonly string _environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? Environments.Production;
    private readonly LoggingLevelSwitch _loggingLevelSwitch = new();
    private readonly string _logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LudereAI", "Logs");

    public App()
    {
        Directory.CreateDirectory(_logDirectory);
        _loggingLevelSwitch.MinimumLevel = _environment == Environments.Development ? LogEventLevel.Debug : LogEventLevel.Information;
        _loggingLevelSwitch.MinimumLevel = Environment.GetEnvironmentVariable("LOG_LEVEL") switch
        {
            "Debug" => LogEventLevel.Debug,
            "Information" => LogEventLevel.Information,
            "Warning" => LogEventLevel.Warning,
            "Error" => LogEventLevel.Error,
            "Fatal" => LogEventLevel.Fatal,
            _ => _loggingLevelSwitch.MinimumLevel
        };
        
        if (_environment == Environments.Development) return;
        
        SentrySdk.Init(options =>
        {
            options.Dsn = "https://4e80dccaaa4336baf45d556d80cebbc6@o4506301335142400.ingest.us.sentry.io/4508719728558080";
            options.Debug = false;
            options.TracesSampleRate = 0.5;
            options.ProfilesSampleRate = 0.5;
            options.IsGlobalModeEnabled = true;
            options.SendDefaultPii = true;
            options.AttachStacktrace = true;
            options.AutoSessionTracking = true;
            options.Environment = _environment;
            
            options.AddIntegration(new ProfilingIntegration());
        });
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += App_DispatcherUnhandledException;

        Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .ConfigureServices((_, collection) =>
            {
                ConfigureServices(collection);
            })
            .UseSerilog((_, config) =>
                {
                    config
                        .Enrich.WithThreadId()
                        .Enrich.WithProcessId()
                        .Enrich.WithMachineName()
                        .Enrich.FromLogContext()
                        .MinimumLevel.ControlledBy(_loggingLevelSwitch)
                        .WriteTo.Sentry(o => o.InitializeSdk = false)
                        .WriteTo.Console(
                            outputTemplate:
                            "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] [{ThreadId}] [{MachineName}:{ProcessId}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
                            theme: AnsiConsoleTheme.Literate)
                        .WriteTo.File(
                            Path.Combine(_logDirectory, "log-.txt"),
                            rollingInterval: RollingInterval.Day,
                            outputTemplate:
                            "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] [{ThreadId}] [{MachineName}:{ProcessId}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
                            retainedFileCountLimit: 7,
                            fileSizeLimitBytes: 10 * 1024 * 1024
                        );
                }
            ).Build();
        
        Log.Information("Logging started at {Now}", DateTime.UtcNow.ToString("u"));
        Log.Information("Log directory: {Directory}", _logDirectory);
        Log.Information("Command line arguments: [ {Args} ]", $"{string.Join(" ", e.Args)}");
        Log.Information("Environment: {Environment}", _environment);
        
        if (_environment == Environments.Development)
        {
            Log.Warning("Development environment detected. Using development API URL: {URL}", DevApiUrl);
            Log.Debug("Sentry is disabled in development environment");
        }
        else if (_environment == Environments.Staging)
        {
            Log.Information("Staging environment detected. Using staging API URL: {URL}", StagApiUrl);
        }
        else
        {
            Log.Information("Production environment detected. Using production API URL: {URL}", ProdApiUrl);
        }
        
        await Host.StartAsync();
        
        SentrySdk.ConfigureScope(scope =>
        {
            scope.AddAttachment(Path.Combine(_logDirectory, "log-.txt"));
        });
        
        var connectivityService = Host.Services.GetRequiredService<IConnectivityService>();
        var navigationService = Host.Services.GetRequiredService<INavigationService>();
        var updateService = Host.Services.GetRequiredService<IUpdateService>();
        
        connectivityService.OnConnectivityChanged += async (sender, IsConnected) =>
        {
            if (IsConnected) return;
            Log.Warning("API connectivity lost. Attempting to reconnect...");
        };

        await updateService.CheckForUpdatesAsync();
        
        var tokenService = Host.Services.GetRequiredService<ITokenService>();
        var authService = Host.Services.GetRequiredService<IAuthService>();
        
        tokenService.TokenInvalidated += async (sender, args) =>
        {
            await authService.LogoutAsync();
        };


        navigationService.ShowWindow<AuthView>();
        
        await connectivityService.StartConnectivityCheck();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // API Client
        services.AddHttpClient("LudereAI", client =>
        {
            if (_environment == Environments.Development)
            {
                client.BaseAddress = new Uri(DevApiUrl);
            }
            else if (_environment == Environments.Staging)
            {
                client.BaseAddress = new Uri(StagApiUrl);
            }
            else
            {
                client.BaseAddress = new Uri(ProdApiUrl);
            }
        });

        // Services
        services.AddSingleton<IConnectivityService, ConnectivityService>();
        services.AddSingleton<IUpdateService, UpdateService>();
        services.AddSingleton<IAPIClient, APIClient>();
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<ISessionService, SessionService>();
        services.AddSingleton<IAssistantService, AssistantService>();
        services.AddSingleton<IGameService, SteamGameService>();
        services.AddSingleton<IScreenshotService, ScreenshotService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<ISubscriptionService, SubscriptionService>();
        services.AddSingleton<IAudioPlaybackService, AudioPlaybackService>();
        services.AddSingleton<IChatService, ChatService>();

        // ViewModels
        services.AddSingleton<AuthViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<SignUpViewModel>();
        services.AddTransient<ChatViewModel>();
        services.AddTransient<GameViewModel>();

        // Views
        services.AddTransient<AuthView>();
        services.AddTransient<LoginView>();
        services.AddTransient<SignUpView>();
        services.AddTransient<ChatView>();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        try
        {
            if (Host == null) return;
                
            await Host.StopAsync();
            Host.Dispose();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during application shutdown");
            SentrySdk.CaptureException(ex);
        }
        finally
        {
            await Log.CloseAndFlushAsync();
            base.OnExit(e);
        }
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unhandled application exception");
        SentrySdk.CaptureException(e.Exception);
        e.Handled = true;
    }
}