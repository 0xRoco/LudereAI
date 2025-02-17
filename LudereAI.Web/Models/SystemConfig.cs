namespace LudereAI.Web.Models;

public class SystemConfig
{
    public ApiSettings ApiSettings { get; set; }
    public SecuritySettings SecuritySettings { get; set; }
    public GeneralSettings GeneralSettings { get; set; }
}

public class ApiSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public int Timeout { get; set; }
    public int MaxRetries { get; set; }
}

public class SecuritySettings
{
    public int PasswordMinLength { get; set; }
    public bool RequireUpperCase { get; set; }
    public bool RequireSpecialChars { get; set; }
    public int SessionsTimeout { get; set; }
}

public class GeneralSettings
{
    public bool IsWaitlistActive { get; set; }
    public bool MaintenanceMode { get; set; }
}