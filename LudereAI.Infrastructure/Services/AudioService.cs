using ElevenLabs;
using ElevenLabs.Models;
using ElevenLabs.TextToSpeech;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Domain.Models.Configs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LudereAI.Infrastructure.Services;

public class AudioService : IAudioService
{
    private readonly ILogger<IAudioService> _logger;
    private readonly ElevenLabsConfig _elevenLabsConfig;
    private readonly IFeatureFlagsService _featureFlagsService;

    public AudioService(ILogger<IAudioService> logger, IOptions<ElevenLabsConfig> elevenLabsConfig, IFeatureFlagsService featureFlagsService)
    {
        _logger = logger;
        _featureFlagsService = featureFlagsService;
        _elevenLabsConfig = elevenLabsConfig.Value;
        
    }

    public async Task<byte[]> GenerateAudio(string text)
    {
        try
        {
            if (!_featureFlagsService.IsFeatureEnabled("Assistant.VoiceGenerationEnabled"))
            {
                return [];
            }
            
            var elevenLabsClient = new ElevenLabsClient(_elevenLabsConfig.ApiKey);
            var voice = await elevenLabsClient.VoicesEndpoint.GetVoiceAsync(_elevenLabsConfig.Voice);
            var request = new TextToSpeechRequest(voice, text, model: Model.EnglishTurboV2);
            var audio = await elevenLabsClient.TextToSpeechEndpoint.TextToSpeechAsync(request);
            return audio == null ? [] : audio.ClipData.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Voice generation process failed");
            return [];
        }

    }
}