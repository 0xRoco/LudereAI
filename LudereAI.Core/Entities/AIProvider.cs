using LudereAI.Shared.Enums;

namespace LudereAI.Core.Entities;

public class AIProvider
{
    public AIProviderType ProviderType { get; set; } = AIProviderType.OpenAI;
    public string ApiKey { get; set; }  = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;

    public bool IsCustom => ProviderType == AIProviderType.Custom;
    
    public bool IsValid()
    {
        return ProviderType != 0 && 
               !string.IsNullOrWhiteSpace(ApiKey) && 
               !string.IsNullOrWhiteSpace(BaseUrl) && 
               !string.IsNullOrWhiteSpace(Model);
    }

    public override string ToString()
    {
        return $"{ProviderType}";
    }
}