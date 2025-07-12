using LudereAI.Core.Entities;
using LudereAI.Shared.Enums;

namespace LudereAI.Services;

public static class AIProviderFactory
{
    public static List<AIProvider> GetAvailableProviders()
    {
        return
        [
            new AIProvider
            {
                ProviderType = AIProviderType.OpenAI,
                BaseUrl = "https://api.openai.com/v1/",
                Model = "gpt-4o-mini"
            },
            new AIProvider
            {
                ProviderType = AIProviderType.Google,
                BaseUrl = "https://generativelanguage.googleapis.com/v1beta/",
                Model = "gemini-2.5-flash"
            },
            new AIProvider
            {
                ProviderType = AIProviderType.Custom,
                BaseUrl = "http://localhost:1234/v1/",
                Model = "custom-model"
            }
        ];
    }
}