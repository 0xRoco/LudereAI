using LudereAI.Shared.Enums;
using LudereAI.Shared.Models;

namespace LudereAI.Core.Entities;

public class AppSettings
{
    public GeneralSettings General { get; set; } = new();
    public GameIntegrationSettings GameIntegration { get; set; } = new();
    public KeyBindSettings KeyBind { get; set; } = new();
    public AdvancedSettings Advanced { get; set; } = new();
}

public class GeneralSettings
{
    public AIProvider? AIProvider { get; set; } = new();
    public string Theme { get; set; } = "Dark";
    public string Language { get; set; } = "English";
    public bool TextToSpeechEnabled { get; set; } = true;
    public bool AutoStartWithWindows { get; set; } = false;
    public bool MinimizeToTray { get; set; } = false;
}

public class GameIntegrationSettings
{
    public bool Enabled { get; set; } = false;
    public int ScanInterval { get; set; } = 3; // in seconds
    public bool AutoCaptureScreenshots { get; set; } = true;
}

public class KeyBindSettings
{
    public List<KeyBinding> Hotkeys { get; set; } =
    [
        new()
        {
            Id = "ToggleOverlay", Name = "Toggle Overlay", Key = LudereKey.O, Modifiers = LudereModifierKeys.Alt,
            IsGlobal = true, IsEnabled = true
        },

        new()
        {
            Id = "NewChat", Name = "New Chat", Key = LudereKey.N, Modifiers = LudereModifierKeys.Control,
            IsGlobal = true, IsEnabled = true
        }
    ];
}

public class AdvancedSettings
{
    public bool FirstTimeSetupCompleted { get; set; } = false;
    public bool EnableDebugLogging { get; set; } = false;
    public bool EnableExperimentalFeatures { get; set; } = false;
}