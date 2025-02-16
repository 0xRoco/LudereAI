using System.ClientModel;
using LudereAI.Application.Interfaces;
using LudereAI.Domain.Models.AI;
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
    private readonly AIBaseConfig _openAIConfig;
    private readonly AIBaseConfig _deepSeekConfig;
    private readonly AIBaseConfig _geminiConfig;

    public ChatClientFactory(ILogger<IChatClientFactory> logger, IOptions<AIConfig> config)
    {
        _logger = logger;
        _config = config.Value;
        
        _openAIConfig = _config.OpenAI;
        _deepSeekConfig = _config.DeepSeek;
        _geminiConfig = _config.Gemini;
    }

    public ChatClient CreateOpenAIClient()
    {
        var options = new OpenAIClientOptions();
        var client = new OpenAIClient(new ApiKeyCredential(_openAIConfig.APIKey), options);

        return client.GetChatClient(_openAIConfig.Model);
    }

    public ChatClient CreateGeminiClient()
    {
        var client = new OpenAIClient(new ApiKeyCredential(_geminiConfig.APIKey), new OpenAIClientOptions()
        {
            Endpoint = new Uri(_geminiConfig.Endpoint)
        });
        
        return client.GetChatClient(_geminiConfig.Model);
    }

    public AudioClient CreateAudioClient()
    {
        var options = new OpenAIClientOptions();
        options.Endpoint = new Uri("https://ai.mdnite.dev/v1/");
        var voice = new AudioClient("voice-en-us-ryan-high", new ApiKeyCredential("empty"), options);
        return voice;
    }
}