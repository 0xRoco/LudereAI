using System.IO;
using System.Windows;
using System.Windows.Threading;
using LudereAI.Core.Interfaces;
using LudereAI.Core.Interfaces.Repositories;
using LudereAI.Core.Interfaces.Services;
using LudereAI.Services;
using LudereAI.Services.Repositories;
using LudereAI.Services.Services;
using LudereAI.WPF.Infrastructure;
using LudereAI.WPF.Interfaces;
using LudereAI.WPF.Services;
using LudereAI.WPF.ViewModels;
using LudereAI.WPF.Views;
using Microsoft.EntityFrameworkCore;
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
public partial class App
{
    public IHost? Host;

    private readonly string _environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? Environments.Production;
    private readonly LoggingLevelSwitch _loggingLevelSwitch = new();
    private readonly string _logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LudereAI", "Logs");

    public App()
    {
        try
        {
            Directory.CreateDirectory(_logDirectory);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to create log directory: {ex.Message}", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }


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
            )
            .ConfigureServices(ConfigureServices)
            .Build();
        
        Log.Information("Logging started at {Now}", DateTime.UtcNow.ToString("u"));
        Log.Information("Log directory: {Directory}", _logDirectory);
        Log.Information("Command line arguments: [ {Args} ]", $"{string.Join(" ", e.Args)}");
        Log.Information("Environment: {Environment}", _environment);
        
        await Host.StartAsync();
        
        using (var scope = Host.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            await dbContext.Database.EnsureCreatedAsync();
        }
        
        var navigationService = Host.Services.GetRequiredService<INavigationService>();
        var settingsService = Host.Services.GetRequiredService<ISettingsService>();
        var settings = await settingsService.LoadSettings();
        
        if (settings.Advanced.FirstTimeSetupCompleted)
        {
            navigationService.ShowWindow<ChatView>();
        }
        else
        {
            navigationService.ShowWindow<SetupView>();
        }
        
        settingsService.ApplySettings(settings);
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<DatabaseContext>(options =>
        {
            var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LudereAI", "LudereAI.db");
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath) ?? throw new InvalidOperationException("Unable to create database directory."));
            options.UseSqlite($"Data Source={dbPath};");
            options.EnableDetailedErrors();
            options.EnableSensitiveDataLogging();
        }, ServiceLifetime.Transient);

        services.AddAutoMapper(typeof(CoreMappingProfile));
        
        // --- Global State / UI Services ---
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IInputService, InputService>();
        services.AddSingleton<IOverlayService, OverlayService>();
        services.AddSingleton<IAudioService, AudioService>();

        // --- Operational Services ---
        services.AddTransient<IGameService, GameService>();
        services.AddTransient<IChatService, ChatService>();
        services.AddTransient<IOpenAIService, OpenAIService>();
        services.AddTransient<IScreenshotService, ScreenshotService>();

        // Repositories
        services.AddTransient<IConversationRepository, ConversationRepository>();
        services.AddTransient<IMessageRepository, MessageRepository>();

        // Factories and Handlers
        services.AddTransient<IInstructionLoader, InstructionLoader>();
        services.AddTransient<IChatClientFactory, ChatClientFactory>();
        services.AddTransient<IMessageHandler, MessageHandler>();

        // --- ViewModels ---
        services.AddTransient<SetupViewModel>();
        services.AddTransient<ChatViewModel>();
        services.AddTransient<SettingsViewModel>();

        // --- Views ---
        services.AddTransient<SetupView>();
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

    private static void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unhandled application exception");
        var result = MessageBox.Show(
            $"An unhandled error occurred: {e.Exception.Message}\n\nDo you want to continue?",
            "Unhandled Error",
            MessageBoxButton.YesNo,
            MessageBoxImage.Error);
            
        e.Handled = result == MessageBoxResult.Yes;
        
        if (!e.Handled)
        {
            Log.Fatal(e.Exception, "Application will exit due to unhandled exception");
            Current.Shutdown();
        }
        else
        {
            Log.Information("User chose to continue after unhandled exception");
        }

    }
}