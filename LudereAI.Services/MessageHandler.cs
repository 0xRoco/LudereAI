using LudereAI.Core.Entities;
using LudereAI.Core.Entities.Chat;
using LudereAI.Core.Entities.Configs;
using LudereAI.Core.Interfaces;
using LudereAI.Core.Interfaces.Repositories;
using LudereAI.Shared.DTOs;
using LudereAI.Shared.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace LudereAI.Services;

public class MessageHandler : IMessageHandler
{
    private readonly ILogger<IMessageHandler> _logger;
    private readonly IMessageRepository _messageRepository;
    private readonly IConversationRepository _conversationRepository;
    private readonly IInstructionLoader _instructionLoader;
    private const int MaxMessageHistory = 60;
    private const int MaxScreenshotHistory = 0;
    private readonly bool _isOpenAI;

    public MessageHandler(IMessageRepository messageRepository, IInstructionLoader instructionLoader, IConversationRepository conversationRepository, IOptions<AIConfig> config, ILogger<IMessageHandler> logger)
    {
        _messageRepository = messageRepository;
        _instructionLoader = instructionLoader;
        _conversationRepository = conversationRepository;
        _logger = logger;
        
        _isOpenAI = config.Value.Name.Equals("openai", StringComparison.CurrentCultureIgnoreCase);
    }

    /*public async Task<StoredFile?> HandleScreenshot(AssistantRequest request, Conversation conversation)
    {
        if (string.IsNullOrWhiteSpace(request.Screenshot))
            return null;

        var storedFile = await _fileStorageService.UploadFileAsync(request.Screenshot, conversation);
        return storedFile;
    }*/

    public async Task<List<ChatMessage>> BuildMessageHistory(AssistantRequest request, Conversation conversation)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(await _instructionLoader.LoadInstructions(request.GameContext))
        };
        

        var recentMessages = conversation.Messages
            .TakeLast(MaxMessageHistory)
            .ToList();

        var screenshotCount = 0;

        foreach (var msg in recentMessages)
        {
            if (msg.Screenshot != null)
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (screenshotCount < MaxScreenshotHistory)
                {
                    messages.Add(await CreateChatMessage(msg.Content, msg.Role, msg.Screenshot.Base64));
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

    
    //TODO: Move this to IChatService (IOpenAIService should not be handling database operations)
    public async Task SaveConversationMessages(AssistantRequest request, AIResponse response)
    {
        var screenshotId = Guid.NewGuid().ToString("N");
        
        var userMessage = new Message
        {
            ConversationId = response.ConversationId,
            ScreenshotId = screenshotId,
            Content = request.Message,
            Role = MessageRole.User,
        };
        
        if (!string.IsNullOrWhiteSpace(request.Screenshot))
        {
            userMessage.Screenshot = new Screenshot
            {
                Id = screenshotId,
                MessageId = userMessage.Id,
                Base64 = request.Screenshot
            };
        }

        var assistantMessage = new Message
        {
            Id = response.MessageId,
            ConversationId = response.ConversationId,
            Content = response.Message,
            Role = MessageRole.Assistant
        };


        await _messageRepository.CreateBatch([
            userMessage,
            assistantMessage
        ]);
        
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
            MessageRole.User => new UserChatMessage(new List<ChatMessageContentPart>
            {
               ChatMessageContentPart.CreateTextPart(message),
               ChatMessageContentPart.CreateImagePart(BinaryData.FromBytes(Convert.FromBase64String(screenshot)), "image/jpeg")
            }),
            /*MessageRole.User when _isOpenAI && Uri.TryCreate(screenshot, UriKind.Absolute, out var link) => new
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
            }),*/
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