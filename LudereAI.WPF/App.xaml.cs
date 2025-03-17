using System.IO;
using System.Windows;
using System.Windows.Threading;
using LudereAI.WPF.Interfaces;
using LudereAI.WPF.Mappers;
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

    private const string ApiUrl = "https://localhost:9099/";
    private readonly string _environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? Environments.Production;
    private readonly LoggingLevelSwitch _loggingLevelSwitch = new();
    private readonly string _logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LudereAI", "Logs");

    public App()
    {
        
        /*_environment = Environments.Staging;
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", _environment);*/
        
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
        Log.Information("Using API URL: {ApiUrl}", ApiUrl);
        
        
        await Host.StartAsync();
        
        var connectivityService = Host.Services.GetRequiredService<IConnectivityService>();
        var navigationService = Host.Services.GetRequiredService<INavigationService>();
        var settingsService = Host.Services.GetRequiredService<ISettingsService>();
        
        settingsService.ApplySettings(settingsService.LoadSettings());
        
        connectivityService.OnConnectivityChanged += async (sender, IsConnected) =>
        {
            if (IsConnected) return;
            Log.Warning("API connectivity lost. Attempting to reconnect...");
        };
        
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
            client.BaseAddress = new Uri(ApiUrl);
        });

        services.AddAutoMapper(typeof(ModelsMappingProfile));
        
        // Services
        services.AddSingleton<IConnectivityService, ConnectivityService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IAPIClient, APIClient>();
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<ISessionService, SessionService>();
        services.AddSingleton<IAssistantService, AssistantService>();
        services.AddSingleton<IGameService, GameService>();
        services.AddSingleton<IScreenshotService, ScreenshotService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IAudioPlaybackService, AudioPlaybackService>();
        services.AddSingleton<IChatService, ChatService>();
        services.AddSingleton<IInputService, InputService>();
        services.AddSingleton<IOverlayService, OverlayService>();

        // ViewModels
        services.AddSingleton<AuthViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<SignUpViewModel>();
        services.AddSingleton<ChatViewModel>();
        services.AddTransient<SettingsViewModel>();

        // Views
        services.AddTransient<AuthView>();
        services.AddTransient<LoginView>();
        services.AddTransient<SignUpView>();
        services.AddTransient<ChatView>();
        services.AddTransient<SettingsView>();
        services.AddTransient<OverlayView>();
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
        e.Handled = true;
    }
}