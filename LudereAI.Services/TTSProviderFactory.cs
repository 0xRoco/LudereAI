using LudereAI.Core.Entities;
using LudereAI.Shared.Enums;

namespace LudereAI.Services;

public static class TTSProviderFactory
{
    public static IEnumerable<TTSProvider> GetAvailableProviders()
    {
        return new List<TTSProvider>
        {
            new()
            {
                ProviderType = TTSProviderType.Windows
            },
            new()
            {
                ProviderType = TTSProviderType.ElevenLabs,
                VoiceId = "asDeXBMC8hUkhqqL7agO"
            },
            new()
            {
                ProviderType = TTSProviderType.OpenAI,
                VoiceId = "%ChangeMe%"
            },
            new()
            {
                ProviderType = TTSProviderType.Piper,
                BaseUrl = "http://localhost:5002/v1/",
                VoiceId = "voice-en-us-ryan-high"
            },
            new()
            {
                ProviderType = TTSProviderType.Custom,
                BaseUrl = "http://localhost:5000/api/tts",
                VoiceId = "default-voice",
                ApiKey = "%ChangeMe%"
            }
        };
    }
}