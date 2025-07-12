using System.ClientModel;
using System.Speech.Synthesis;
using ElevenLabs;
using ElevenLabs.Models;
using ElevenLabs.TextToSpeech;
using LudereAI.Core.Entities;
using LudereAI.Core.Interfaces.Services;
using LudereAI.Shared.Enums;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Audio;

namespace LudereAI.Services.Services;

public class TextToSpeechService : ITextToSpeechService
{
    private readonly ILogger<ITextToSpeechService> _logger;
    private readonly ISettingsService _settingsService;
    private TTSProvider _ttsProvider;


    public TextToSpeechService(ILogger<ITextToSpeechService> logger, ISettingsService settingsService)
    {
        _logger = logger;
        _settingsService = settingsService;

        _settingsService.OnSettingsApplied += settings =>
        {
            _ttsProvider = settings.General.TTSProvider;
            _logger.LogInformation($"TTS Service configured with provider: {_ttsProvider.ProviderType}");
        };
        
        _ttsProvider = _settingsService.Settings.General.TTSProvider;
    }
    
    
    public async Task<byte[]> GenerateTTS(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || !_ttsProvider.IsValid()) return [];

        return _ttsProvider.ProviderType switch
        {
            TTSProviderType.Windows => GenerateWindowsTTS(text),
            TTSProviderType.OpenAI => await GenerateOpenAITTS(text),
            TTSProviderType.ElevenLabs => await GenerateElevenLabsTTS(text),
            TTSProviderType.Piper => await GeneratePiperTTS(text),
            TTSProviderType.Custom => [], // Could be implemented similar to Piper where custom is an OpenAI library compatible endpoint
            _ => []
        };
    }

    private byte[] GenerateWindowsTTS(string text)
    {
        _logger.LogDebug("Generating audio using Windows TTS for text: {Text}", text);
        using var synthesizer = new SpeechSynthesizer();
        using var stream = new MemoryStream();
        synthesizer.SetOutputToWaveStream(stream);
        synthesizer.Speak(text);
        _logger.LogDebug("Audio generation completed using Windows TTS.");
        return stream.ToArray();
    }

    private async Task<byte[]> GenerateOpenAITTS(string text)
    {
        _logger.LogDebug("Generating audio using OpenAI TTS for text: {Text}", text);
        
        // Assuming the provided AI API key is the same for the TTS
        var aiSettings = _settingsService.Settings.General.AIProvider;
        if (!aiSettings.IsValid()) return [];

        var client = new OpenAIClient(aiSettings.ApiKey);
        var audioClient = client.GetAudioClient(_ttsProvider.VoiceId);
        var response = await audioClient.GenerateSpeechAsync(text, _ttsProvider.VoiceId);
        return response.Value.ToArray();
    }

    private async Task<byte[]> GenerateElevenLabsTTS(string text)
    {
        _logger.LogDebug("Generating audio using ElevenLabs TTS for text: {Text}", text);
        var client = new ElevenLabsClient(_ttsProvider.ApiKey);
        var voice = await client.VoicesEndpoint.GetVoiceAsync(_ttsProvider.VoiceId);
        var ttsRequest = new TextToSpeechRequest(voice, text, model: Model.EnglishTurboV2);
        var result = await client.TextToSpeechEndpoint.TextToSpeechAsync(ttsRequest);
        return result.ClipData.ToArray();
    }
    
    private async Task<byte[]> GeneratePiperTTS(string text)
    {
        _logger.LogDebug("Generating audio using Piper TTS for text: {Text}", text);
        var options = new OpenAIClientOptions { Endpoint = new Uri(_ttsProvider.BaseUrl) };
        // Piper instance could be configured with an API key
        var credential = string.IsNullOrWhiteSpace(_ttsProvider.ApiKey)
            ? new ApiKeyCredential("dummy")
            : new ApiKeyCredential(_ttsProvider.ApiKey);

        var client = new AudioClient(_ttsProvider.VoiceId, credential, options);
        var response = await client.GenerateSpeechAsync(text, new GeneratedSpeechVoice());
        return response.Value.ToArray();
    }
}