using LudereAI.Shared.Enums;

namespace LudereAI.Core.Entities;

public class AIProvider
{
    public AIProviderType ProviderType { get; set; }
    public string Name { get; set; }
    public string ApiKey { get; set; }
    public string BaseUrl { get; set; }
    public string Model { get; set; }
    public bool IsEnabled { get; set; }

    public AIProvider(AIProviderType providerType, string name, string apiKey, string baseUrl, string model, bool isEnabled)
    {
        ProviderType = providerType;
        Name = name;
        ApiKey = apiKey;
        BaseUrl = baseUrl;
        Model = model;
        IsEnabled = isEnabled;
    }

    public AIProvider(AIProviderType providerType, string name)
    {
        ProviderType = providerType;
        Name = name;
        ApiKey = string.Empty;
        BaseUrl = string.Empty;
        Model = string.Empty;
        IsEnabled = false;
    }

    public override string ToString()
    {
        return $"{Name}";
    }
}