using System.IO;
using LudereAI.Shared;
using LudereAI.WPF.Interfaces;
using LudereAI.WPF.Models;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace LudereAI.WPF.Services;

public class SettingsService : ISettingsService
{
    private readonly ILogger<ISettingsService> _logger;
    private readonly IChatService _chatService;
    private readonly IInputService _inputService;
    private readonly string _settingsPath;
    
    public SettingsService(ILogger<ISettingsService> logger, IChatService chatService, IInputService inputService)
    {
        _logger = logger;
        _chatService = chatService;
        _inputService = inputService;

        _settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LudereAI", "settings.json");
    }

    public AppSettings LoadSettings()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                var defaultSettings = new AppSettings();
                SaveSettings(defaultSettings);
                return defaultSettings;
            }
            
            var json = File.ReadAllText(_settingsPath);
            var settings = json.FromJson<AppSettings>();
            return settings ?? new AppSettings();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load settings");
            return new AppSettings();
        }
    }

    public void SaveSettings(AppSettings settings)
    {
        try
        {
            var directory = Path.GetDirectoryName(_settingsPath);
            if (!Directory.Exists(directory) && !string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            var json = settings.ToJson();
            File.WriteAllText(_settingsPath, json);
            
            _logger.LogInformation("Settings saved successfully");
            
            
            ApplySettings(settings);
        }catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save settings");
        }
    }

    public void ApplySettings(AppSettings settings)
    {
        try
        {
            ApplyKeyBindSettings(settings.KeyBind);
            SetAutoStart(settings.General.AutoStartWithWindows, settings.General.MinimizeToTray);
            ApplyTheme(settings.General.Theme);
            ApplyPrivacySettings(settings.Privacy);
            
            _chatService.SetAutoCaptureScreenshots(settings.GameIntegration.AutoCaptureScreenshots);
            _chatService.SetTextToSpeechEnabled(settings.General.TextToSpeechEnabled);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply settings");
        }
    }

    private void SetAutoStart(bool enable, bool silent = false)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            if (key == null) return;
            
            var appPath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
            if (string.IsNullOrWhiteSpace(appPath)) return;

            if (enable)
            {
                if (silent)
                    key.SetValue("LudereAI", $"\"{appPath}\" --silent");
                else
                    key.SetValue("LudereAI", appPath);
            }
            else
                key.DeleteValue("LudereAI", false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set auto start");
        }
    }

    private void ApplyTheme(string theme)
    {
        try
        {
            var paletteHelper = new PaletteHelper();
            var appTheme = paletteHelper.GetTheme();
            var baseTheme = theme switch
            {
                "Light" => BaseTheme.Light,
                "Dark" => BaseTheme.Dark,
                "System" => BaseTheme.Inherit,
                _ => BaseTheme.Dark
            };
            
            appTheme.SetBaseTheme(baseTheme);
            
            paletteHelper.SetTheme(appTheme);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply theme");
        }
    }
    
    private void ApplyPrivacySettings(PrivacySettings privacySettings)
    {
        try
        {
            if (privacySettings.AllowUsageStatistics)
            {
                if (!SentrySdk.IsEnabled && Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == Environments.Production)
                {
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
                        options.Environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? Environments.Production;
            
                        options.AddIntegration(new ProfilingIntegration());
                        
                    });
                }
            }
            else
            {
                SentrySdk.Close();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply privacy settings");
        }
    }

    private void ApplyKeyBindSettings(KeyBindSettings settings)
    {
        try
        {
            _logger.LogInformation("Applying keybind settings with {count} hotkeys", settings.Hotkeys.Count);
            
            _inputService.UnregisterAllHotkeys();
            
            foreach (var binding in settings.Hotkeys)
            {
                _inputService.RegisterHotkey(binding);
                _logger.LogDebug("Registered hotkey {id}: {key}", binding.Id, binding);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply key bind settings");
        }
    }
}