﻿using System.ClientModel;
using ElevenLabs;
using ElevenLabs.Models;
using ElevenLabs.TextToSpeech;
using LudereAI.Application.Interfaces;
using LudereAI.Domain.Models.Configs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Audio;
using OpenAI.Chat;

namespace LudereAI.Infrastructure;

public class ChatClientFactory : IChatClientFactory
{
    private readonly ILogger<IChatClientFactory> _logger;
    private readonly AIConfig _config;
    private readonly PiperConfig _piperConfig;
    private readonly ElevenLabsConfig _elevenLabsConfig;
    private bool _isOpenAI;

    public ChatClientFactory(ILogger<IChatClientFactory> logger, IOptions<AIConfig> config, IOptions<PiperConfig> piperConfig, IOptions<ElevenLabsConfig> elevenLabsConfig)
    {
        _logger = logger;
        _config = config.Value;
        _piperConfig = piperConfig.Value;
        _elevenLabsConfig = elevenLabsConfig.Value;
        
        _isOpenAI = _config.Name.Equals("openai", StringComparison.CurrentCultureIgnoreCase);
    }
    
    public ChatClient CreateChatClient()
    {
        var client = new OpenAIClient(new ApiKeyCredential(_config.ApiKey), new OpenAIClientOptions()
        {
            Endpoint = new Uri(_config.Endpoint)
        });
        
        return client.GetChatClient(_config.Model);
    }

    public async Task<byte[]> GenerateAudio(string text)
    {
        if (!IsPiperConfigured() && !IsElevenLabsConfigured())
        {
            return [];
        }

        if (IsPiperConfigured())
        {
            var piper = CreatePiperClient();
            var audio = await piper.GenerateSpeechAsync(text, new GeneratedSpeechVoice());
            
            return audio.Value.ToArray();
        }

        if (!IsElevenLabsConfigured()) return [];
        {
            var elevenLabsClient = new ElevenLabsClient(_elevenLabsConfig.ApiKey);
            var voice = await elevenLabsClient.VoicesEndpoint.GetVoiceAsync(_elevenLabsConfig.Voice);
            var request = new TextToSpeechRequest(voice, text, model: Model.EnglishTurboV2);
            var audio = await elevenLabsClient.TextToSpeechEndpoint.TextToSpeechAsync(request);
            return audio == null ? [] : audio.ClipData.ToArray();
        }

    }
    
    private bool IsPiperConfigured()
    {
        return !string.IsNullOrWhiteSpace(_piperConfig.Endpoint) && !string.IsNullOrWhiteSpace(_piperConfig.ApiKey) && !string.IsNullOrWhiteSpace(_piperConfig.Voice);
    }
    
    private bool IsElevenLabsConfigured()
    {
        return !string.IsNullOrWhiteSpace(_elevenLabsConfig.ApiKey) && !string.IsNullOrWhiteSpace(_elevenLabsConfig.Voice);
    }

    private AudioClient CreatePiperClient()
    {
        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri(_piperConfig.Endpoint)
        };
        var voice = new AudioClient(_piperConfig.Voice, new ApiKeyCredential(_piperConfig.ApiKey), options);
        return voice;
    }
    
}