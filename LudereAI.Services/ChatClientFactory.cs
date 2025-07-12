using System.ClientModel;
using LudereAI.Core.Entities;
using LudereAI.Core.Interfaces;
using LudereAI.Core.Interfaces.Services;
using LudereAI.Shared.Enums;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;

namespace LudereAI.Services;

public class ChatClientFactory : IChatClientFactory
{
    private readonly ILogger<IChatClientFactory> _logger;
    private readonly ISettingsService _settingsService;
    private readonly AIProvider _config;

    public ChatClientFactory(ILogger<IChatClientFactory> logger, ISettingsService settingsService)
    {
        _logger = logger;
        _settingsService = settingsService;
        _config = new AIProvider();

        _settingsService.OnSettingsApplied += settings =>
        {
            _config.ApiKey = settings.General.AIProvider.ApiKey;
            _config.Model = settings.General.AIProvider.Model;
            _config.BaseUrl = settings.General.AIProvider.BaseUrl;
        };
        
        var settings = _settingsService.Settings;
        _config.ApiKey = settings.General.AIProvider.ApiKey;
        _config.Model = settings.General.AIProvider.Model;
        _config.BaseUrl = settings.General.AIProvider.BaseUrl;
    }

    public ChatClient CreateChatClient()
    {
        var client = new OpenAIClient(new ApiKeyCredential(_config.ApiKey), new OpenAIClientOptions()
        {
            Endpoint = new Uri(_config.BaseUrl)
        });

        return client.GetChatClient(_config.Model);
    }
}