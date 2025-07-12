using LudereAI.Shared.Enums;

namespace LudereAI.Core.Entities;

public class TTSProvider
{
    public TTSProviderType ProviderType { get; set; } = TTSProviderType.Windows;
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string VoiceId { get; set; } = string.Empty;

    public bool IsValid()
    {
        return ProviderType switch
        {
            TTSProviderType.Windows => true,
            TTSProviderType.OpenAI => !string.IsNullOrWhiteSpace(ApiKey) && !string.IsNullOrWhiteSpace(VoiceId),
            TTSProviderType.ElevenLabs => !string.IsNullOrWhiteSpace(ApiKey) && !string.IsNullOrWhiteSpace(VoiceId),
            TTSProviderType.Piper => !string.IsNullOrWhiteSpace(BaseUrl) && !string.IsNullOrWhiteSpace(VoiceId),
            TTSProviderType.Custom => !string.IsNullOrWhiteSpace(ApiKey) &&
                                      !string.IsNullOrWhiteSpace(BaseUrl) &&
                                      !string.IsNullOrWhiteSpace(VoiceId),
            _ => false
        };
    }

    public bool RequiresApiKey => ProviderType is TTSProviderType.OpenAI or TTSProviderType.ElevenLabs;
    public bool RequiresVoice => ProviderType is TTSProviderType.OpenAI or TTSProviderType.ElevenLabs or TTSProviderType.Piper or TTSProviderType.Custom;
    public bool RequiresBaseUrl => ProviderType is TTSProviderType.Piper or TTSProviderType.Custom;

    public override string ToString()
    {
        return $"{ProviderType}";
    }
}