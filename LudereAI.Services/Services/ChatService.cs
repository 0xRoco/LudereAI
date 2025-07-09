using AutoMapper;
using LudereAI.Core.Entities;
using LudereAI.Core.Entities.Chat;
using LudereAI.Core.Interfaces.Repositories;
using LudereAI.Core.Interfaces.Services;
using LudereAI.Shared;
using LudereAI.Shared.DTOs;
using LudereAI.Shared.Models;
using Microsoft.Extensions.Logging;

namespace LudereAI.Services.Services;

public class ChatService : IChatService
{
    private ILogger<IChatService> _logger;
    private IMapper _mapper;
    private readonly ISettingsService _settingsService;
    private readonly IOpenAIService _openAIService;
    private readonly IScreenshotService _screenshotService;
    private readonly IConversationRepository _conversationRepository;
    private bool _autoCaptureScreenshots;
    private bool _textToSpeechEnabled;
    
    private void SetAutoCaptureScreenshots(bool enabled) => _autoCaptureScreenshots = enabled;
    private void SetTextToSpeechEnabled(bool enabled) => _textToSpeechEnabled = enabled;
    
    public ChatService(ILogger<IChatService> logger, IMapper mapper, ISettingsService settingsService, IConversationRepository conversationRepository, IOpenAIService openAIService, IScreenshotService screenshotService)
    {
        _logger = logger;
        _mapper = mapper;
        _settingsService = settingsService;
        _conversationRepository = conversationRepository;
        _openAIService = openAIService;
        _screenshotService = screenshotService;

        _settingsService.OnSettingsApplied += ApplySettings;
    }

    public async Task<Result<Conversation, IChatService.ChatRequestResult>> SendMessage(ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return Result<Conversation, IChatService.ChatRequestResult>.Error(IChatService.ChatRequestResult.Error, "Message cannot be empty");
        }
        if (string.IsNullOrWhiteSpace(request.GameContext))
        {
            return Result<Conversation, IChatService.ChatRequestResult>.Error(IChatService.ChatRequestResult.Error, "Game context cannot be empty");
        }

        var screenshotBase64 = "";
        
        if (_autoCaptureScreenshots && request.Window != null)
        {
            screenshotBase64 = _screenshotService.GetBase64Screenshot(request.Window.Handle);
        }
        

        var assistantRequest = new AssistantRequest
        {
            ConversationId = request.ConversationId ?? "",
            Message = request.Message,
            Screenshot = screenshotBase64,
            GameContext = request.GameContext,
            TextToSpeechEnabled = _textToSpeechEnabled
        };

        var conversation = await _conversationRepository.Get(assistantRequest.ConversationId)
                           ?? new Conversation { GameContext = assistantRequest.GameContext };

        if (string.IsNullOrWhiteSpace(request.ConversationId))
        {
            await _conversationRepository.Create(conversation);
        }
        
        var aiResponse = await _openAIService.SendMessage(conversation, assistantRequest);

        var updatedConversation = await _conversationRepository.Get(aiResponse.ConversationId);

        if (updatedConversation != null)
        {
            return Result<Conversation, IChatService.ChatRequestResult>.Success(updatedConversation);
        }

        return Result<Conversation, IChatService.ChatRequestResult>.Error(IChatService.ChatRequestResult.Error, "Failed to retrieve updated conversation.");

    }

    public async Task<IEnumerable<Conversation>> GetConversations()
    {
        var conversations = await _conversationRepository.GetAll();
        return conversations;
    }

    private void ApplySettings(AppSettings settings)
    {
        SetAutoCaptureScreenshots(settings.GameIntegration.AutoCaptureScreenshots);
        SetTextToSpeechEnabled(settings.General.TextToSpeechEnabled);
    }
}