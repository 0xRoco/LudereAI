using System.ClientModel;
using LudereAI.Application.Interfaces;
using LudereAI.Domain.Models.Configs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace LudereAI.Infrastructure;

public class ChatClientFactory : IChatClientFactory
{
    private readonly ILogger<IChatClientFactory> _logger;
    private readonly OpenAIConfig _config;

    public ChatClientFactory(ILogger<IChatClientFactory> logger, IOptions<OpenAIConfig> config)
    {
        _logger = logger;
        _config = config.Value;
    }

    public ChatClient CreateChatClient()
    {
        var options = new OpenAIClientOptions();
        /*if (!string.IsNullOrWhiteSpace(model))
        {
            options.Endpoint = new Uri(endpoint);
        }*/
        
        var client = new OpenAIClient(new ApiKeyCredential(_config.ApiKey), options);

        return client.GetChatClient(_config.Model);
    }
}