namespace LudereAI.Shared.DTOs;

public class UpdateInfoDTO
{
    public Version Version { get; set; } = new();
    public string DownloadUrl { get; set; }
    public string Changelog { get; set; }
    public string InstallerName { get; set; }
    public string Hash { get; set; }
    public bool IsCritical { get; set; }
}