namespace LudereAI.WPF.Models;

public class AppSettings
{
    public GeneralSettings General { get; set; } = new();
    public GameIntegrationSettings GameIntegration { get; set; } = new();
    public PrivacySettings Privacy { get; set; } = new();
}

public class GeneralSettings
{
    public string Theme { get; set; } = "Dark";
    public string Language { get; set; } = "English";
    public bool AutoStartWithWindows { get; set; } = false;
    public bool MinimizeToTray { get; set; } = false;
    public bool AutoCheckForUpdates { get; set; } = true;
}

public class GameIntegrationSettings
{
    public bool Enabled { get; set; } = true;
    public int ScanInterval { get; set; } = 3; // in seconds
    public bool AutoCaptureScreenshots { get; set; } = true;
}

public class PrivacySettings
{
    public bool AllowUsageStatistics { get; set; } = true; 
    public bool AllowScreenshotStorage { get; set; } = true;
}