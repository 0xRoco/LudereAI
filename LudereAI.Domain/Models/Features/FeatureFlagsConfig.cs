namespace LudereAI.Domain.Models.Features;

public class FeatureFlagsConfig
{
    public AuthFeatures Auth { get; set; } = new();
    public AssistantFeatures Assistant { get; set; } = new();
    public bool MaintenanceMode { get; set; }
}