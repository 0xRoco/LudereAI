using System.Security.Cryptography;
using System.Text;
using LudereAI.Core.Entities;
using LudereAI.Core.Interfaces.Services;
using LudereAI.Shared;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace LudereAI.Services.Services;

public class SettingsService(ILogger<ISettingsService> logger, IInputService inputService)
    : ISettingsService
{
    private readonly string _settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LudereAI", "settings.dat");

    public event Action<AppSettings>? OnSettingsApplied;
    public AppSettings Settings { get; private set; } = new();

    public async Task SaveSettings(AppSettings settings, CancellationToken ct)
    {
        try
        {
            var directory = Path.GetDirectoryName(_settingsPath);
            if (!Directory.Exists(directory) && !string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            var json = settings.ToJson();

            var encryptedJson = ProtectedData.Protect(
                Encoding.UTF8.GetBytes(json),
                null,
                DataProtectionScope.CurrentUser);
            
            await File.WriteAllBytesAsync(_settingsPath, encryptedJson, ct);
            
            logger.LogInformation("Settings saved successfully");
        }catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save settings");
        }
    }

    public async Task<AppSettings> LoadSettings()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                var defaultSettings = new AppSettings();
                await SaveSettings(defaultSettings, CancellationToken.None);
                return defaultSettings;
            }
            
            var encryptedJson = await File.ReadAllBytesAsync(_settingsPath);
            var json = Encoding.UTF8.GetString(ProtectedData.Unprotect(
                encryptedJson,
                null,
                DataProtectionScope.CurrentUser));
            
            var settings = json.FromJson<AppSettings>();
            return Settings = settings ?? new AppSettings();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load settings");
            return new AppSettings();
        }
    }

    public void ApplySettings(AppSettings settings)
    {
        try
        {
            ApplyKeyBindSettings(settings.KeyBind);
            SetAutoStart(settings.General.AutoStartWithWindows, settings.General.MinimizeToTray);

            OnSettingsApplied?.Invoke(settings);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to apply settings");
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
                key.SetValue("LudereAI", silent ? $"\"{appPath}\" --silent" : appPath);
            }
            else
                key.DeleteValue("LudereAI", false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to set auto start");
        }
    }

    [Obsolete("MaterialDesignThemes WPF is no longer used for theming", true)]
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
            logger.LogError(ex, "Failed to apply theme");
        }
    }
    

    private void ApplyKeyBindSettings(KeyBindSettings settings)
    {
        try
        {
            logger.LogInformation("Applying keybind settings with {count} hotkeys", settings.Hotkeys.Count);
            
            inputService.ApplySettings(settings);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to apply key bind settings");
        }
    }
}