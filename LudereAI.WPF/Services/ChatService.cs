using AutoMapper;
using LudereAI.Shared;
using LudereAI.Shared.DTOs;
using LudereAI.WPF.Interfaces;
using LudereAI.WPF.Models;
using Microsoft.Extensions.Logging;

namespace LudereAI.WPF.Services;

public class ChatService : IChatService
{
    private ILogger<IChatService> _logger;
    private IMapper _mapper;
    private readonly IAssistantService _assistantService;
    private readonly IScreenshotService _screenshotService;

    public ChatService(ILogger<IChatService> logger, IAssistantService assistantService, IScreenshotService screenshotService, IMapper mapper)
    {
        _logger = logger;
        _assistantService = assistantService;
        _screenshotService = screenshotService;
        _mapper = mapper;
    }

    public async Task<Result<Message, IChatService.ChatRequestResult>> SendMessage(ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return Result<Message, IChatService.ChatRequestResult>.Error(IChatService.ChatRequestResult.Error, "Message cannot be empty");
        }
        if (string.IsNullOrWhiteSpace(request.GameContext))
        {
            return Result<Message, IChatService.ChatRequestResult>.Error(IChatService.ChatRequestResult.Error, "Game context cannot be empty");
        }

        var screenshotBase64 = "";
        if (request.Window != null)
        {
            screenshotBase64 = _screenshotService.GetBase64Screenshot(request.Window.Handle);
        }


        var assistantRequest = new AssistantRequestDTO
        {
            ConversationId = request.ConversationId ?? "",
            Message = request.Message,
            Screenshot = screenshotBase64,
            GameContext = request.GameContext
        };
        
        var result = await _assistantService.SendMessage(assistantRequest);
        if (result is { Status: AssistantService.AssistantRequestResult.Success, Value: not null })
        {
            return Result<Message, IChatService.ChatRequestResult>.Success(_mapper.Map<Message>(result.Value));
        }
        
        return Result<Message, IChatService.ChatRequestResult>.Error(IChatService.ChatRequestResult.Error, $"Failed to send message: {result.Message}");
    }

    public async Task<IEnumerable<Conversation>> GetConversations()
    {
        var dtos = await _assistantService.GetConversationsAsync();
        return _mapper.Map<IEnumerable<Conversation>>(dtos);
    }
}