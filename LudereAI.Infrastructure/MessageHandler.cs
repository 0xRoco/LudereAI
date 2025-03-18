using LudereAI.Application.Interfaces;
using LudereAI.Application.Interfaces.Repositories;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Domain.Models;
using LudereAI.Domain.Models.Chat;
using LudereAI.Domain.Models.Media;
using LudereAI.Shared.DTOs;
using LudereAI.Shared.Enums;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;

namespace LudereAI.Infrastructure;

public class MessageHandler : IMessageHandler
{
    private readonly ILogger<IMessageHandler> _logger;
    private readonly IFileStorageService _fileStorageService;
    private readonly IMessageRepository _messageRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IInstructionLoader _instructionLoader;
    private const int MaxMessageHistory = 60;
    private const int MaxScreenshotHistory = 0;
    private bool _isOpenAI;

    public MessageHandler(IFileStorageService fileStorageService, IMessageRepository messageRepository, IInstructionLoader instructionLoader, IConversationRepository conversationRepository, ILogger<IMessageHandler> logger)
    {
        _fileStorageService = fileStorageService;
        _messageRepository = messageRepository;
        _instructionLoader = instructionLoader;
        _conversationRepository = conversationRepository;
        _logger = logger;
    }

    public async Task<StoredFile?> HandleScreenshot(AssistantRequestDTO request, Conversation conversation)
    {
        if (string.IsNullOrWhiteSpace(request.Screenshot))
            return null;

        var storedFile = await _fileStorageService.UploadFileAsync(request.Screenshot, conversation);
        return storedFile;
    }

    public async Task<List<ChatMessage>> BuildMessageHistory(AssistantRequestDTO request, Conversation conversation)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(await _instructionLoader.LoadInstructions(request.GameContext))
        };
        

        var recentMessages = conversation.Messages
            .Take(MaxMessageHistory)
            .Reverse()
            .ToList();

        var screenshotCount = 0;

        foreach (var msg in recentMessages)
        {
            if (msg.Screenshot != null)
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (screenshotCount < MaxScreenshotHistory)
                {
                    messages.Add(await CreateChatMessage(msg.Content, msg.Role, msg.Screenshot.Url));
                    screenshotCount++;
                }
                else
                {
                    messages.Add(new UserChatMessage(msg.Content));
                }
            }
            else
            {
                messages.Add(await CreateChatMessage(msg.Content, msg.Role));
            }
        }

        messages.Add(await (!string.IsNullOrWhiteSpace(request.Screenshot)
            ? CreateChatMessage(request.Message, MessageRole.User, request.Screenshot)
            : CreateChatMessage(request.Message, MessageRole.User)));
        
        return messages;
    }

    public async Task SaveConversationMessages(AssistantRequestDTO request, AIResponse response,
        string screenshotId = "")
    {
        var userMessage = new Message
        {
            ConversationId = response.ConversationId,
            ScreenshotId = screenshotId,
            Content = request.Message,
            Role = MessageRole.User,
        };
        
        if (!string.IsNullOrWhiteSpace(request.Screenshot) && Uri.TryCreate(request.Screenshot, UriKind.Absolute, out _))
        {
            userMessage.Screenshot = new Screenshot
            {
                Id = screenshotId,
                MessageId = userMessage.Id,
                Url = request.Screenshot
            };
        }

        var assistantMessage = new Message
        {
            ConversationId = response.ConversationId,
            Content = response.Message,
            Role = MessageRole.Assistant
        };
        
        
        await _messageRepository.CreateAsync(userMessage);
        await _messageRepository.CreateAsync(assistantMessage);
        
        var conversation = await _conversationRepository.Get(response.ConversationId);
        if (conversation == null)
            return;
        await _conversationRepository.Update(conversation);
    }
    
    private async Task<ChatMessage> CreateChatMessage(string message, MessageRole role, string screenshot = "")
    {
        return role switch
        {
            MessageRole.User when string.IsNullOrWhiteSpace(screenshot) => new UserChatMessage(message),
            MessageRole.User when _isOpenAI && Uri.TryCreate(screenshot, UriKind.Absolute, out var link) => new
                UserChatMessage(new List<ChatMessageContentPart>
                {
                    ChatMessageContentPart.CreateTextPart(message),
                    ChatMessageContentPart.CreateImagePart(link, ChatImageDetailLevel.Auto)
                }),
            MessageRole.User => new UserChatMessage(new List<ChatMessageContentPart>
            {
                ChatMessageContentPart.CreateTextPart(message),
                ChatMessageContentPart.CreateImagePart(BinaryData.FromBytes(await GetBytesFromImageUrl(screenshot)),
                    "image/jpeg")
            }),
            MessageRole.Assistant => new AssistantChatMessage(message),
            MessageRole.System => new SystemChatMessage(message),
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Unsupported message role")
        };
    }

    private async Task<byte[]> GetBytesFromImageUrl(string url)
    {
        using var httpClient = new HttpClient();
        return await httpClient.GetByteArrayAsync(url);
    }
}