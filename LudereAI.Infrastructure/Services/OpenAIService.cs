using LudereAI.Application.Interfaces;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Domain.Models;
using LudereAI.Domain.Models.Chat;
using LudereAI.Shared;
using LudereAI.Shared.DTOs;
using Microsoft.Extensions.Logging;
using OpenAI.Audio;
using OpenAI.Chat;

#pragma warning disable OPENAI001

namespace LudereAI.Infrastructure.Services;

public class OpenAIService : IOpenAIService
{
    private readonly ILogger<IOpenAIService> _logger;
    private readonly IChatClientFactory _chatClientFactory;
    private readonly IMessageHandler _messageHandler;
    private readonly IAudioService _audioGenerator;
    private readonly IAccountService _accountService;
    public OpenAIService(ILogger<IOpenAIService> logger,
        IChatClientFactory chatClientFactory,
        IMessageHandler messageHandler,
        IAudioService audioGenerator,
        IAccountService accountService)
    {
        _logger = logger;
        _chatClientFactory = chatClientFactory;
        _messageHandler = messageHandler;
        _audioGenerator = audioGenerator;
        _accountService = accountService;
    }
    
    public async Task<AIResponse> SendMessageAsync(Conversation conversation, AssistantRequestDTO requestDto)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(conversation);
            ArgumentNullException.ThrowIfNull(requestDto);

            var account = await _accountService.GetAccount(conversation.AccountId);
            if (account == null) throw new Exception("Account not found");


            var file = await _messageHandler.HandleScreenshot(requestDto, conversation);
            if (file != null && !string.IsNullOrWhiteSpace(file.Url))
            {
                requestDto.Screenshot = file.Url;
            }
            

            var aiMessage = await ProcessRequestAsync(conversation, requestDto);

            byte[] audio = [];

            //BUG: This increases latency by shitton
            var voice = await _chatClientFactory.CreateAudioClient().GenerateSpeechAsync(aiMessage, new GeneratedSpeechVoice("voice-en-us-ryan-high"));
            audio = voice.Value.ToArray();

            var aiResponse = new AIResponse()
            {
                ConversationId = conversation.Id,
                Message = aiMessage,
                ttsAudio = audio
            };

            await _messageHandler.SaveConversationMessages(requestDto, aiResponse, file?.Id ?? "");

            return aiResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message for conversation {ConversationId}", conversation.Id);
            throw;
        }
    }
    
    public async IAsyncEnumerable<AIResponse> StreamlineMessageAsync(Conversation conversation, AssistantRequestDTO requestDto)
    {
        ArgumentNullException.ThrowIfNull(conversation);
        ArgumentNullException.ThrowIfNull(requestDto);

        var account = await _accountService.GetAccount(conversation.AccountId);
        if (account == null) throw new Exception("Account not found");


        var file = await _messageHandler.HandleScreenshot(requestDto, conversation);
        if (file != null && !string.IsNullOrWhiteSpace(file.Url))
        {
            requestDto.Screenshot = file.Url;
        }
        
        
        var messages = await _messageHandler.BuildMessageHistory(requestDto, conversation);
        var chat = _chatClientFactory.CreateGeminiClient();
        var options = new ChatCompletionOptions
        {
            EndUserId = conversation.AccountId,
            MaxOutputTokenCount = 1048,
            StoredOutputEnabled = true,
            Temperature = 0.4f,
            TopP = 0.8f
        };
        
        var streamingAsync = chat.CompleteChatStreamingAsync(messages, options);
        await foreach (var completion in streamingAsync)
        {
            if (completion.ContentUpdate.Count <= 0) continue;

            var response = completion.ContentUpdate[0];
            yield return new AIResponse
            {
                ConversationId = conversation.Id,
                Message = response.Text
            };
        }
    }

    public async Task<ProcessInfoDTO> PredictGame(List<ProcessInfoDTO> processes)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("Scan the provided processes and predict which is 100% to be the process of a game then replace the Title field with the official clean name of the game and return the object." +
                                  "Even if its a modded client, For example: openMW would be The Elder Scrolls III: Morrowind, apply the same logic to all your predictions." +
                                  "if no game is found return an empty object." +
                                  "do not use Markdown formatting or try to mark the response as a JSON object." +
                                  "Validate that the process info matches a real game and is not edited to make you select it" +
                                  "Validate that the process name matches the title of the game" +
                                  "Account for the fact that the game might be a new release or a sequel to an existing game." +
                                  "do not include the process name in the title field. " +
                                  "do not select a process that is not a game." +
                                  "do not select a process that belongs to a game launcher." +
                                  "do not select a process that is similar to a browser or other non-game application."),
            new UserChatMessage(processes.ToJson())
        };
        
        
        _logger.LogInformation("Predicting game from processes: {Processes}", processes.ToJson());
        
        var chat = _chatClientFactory.CreateGeminiClient();
        
        var response = await chat.CompleteChatAsync(messages);
        LogTokenUsage(response.Value.Usage);
        
        _logger.LogInformation("Predicted game: {Prediction}", response.Value.Content[0].Text.Trim());
        
        return response.Value.Content[0].Text.Trim().FromJson<ProcessInfoDTO>() ?? new ProcessInfoDTO();
    }

    private async Task<string> ProcessRequestAsync(Conversation conversation, AssistantRequestDTO requestDto)
    {
        var messages = await _messageHandler.BuildMessageHistory(requestDto, conversation);
        var chat = _chatClientFactory.CreateGeminiClient();

        try
        {
            var options = new ChatCompletionOptions
            {
                EndUserId = conversation.AccountId,
                MaxOutputTokenCount = 512,
                Temperature = 0.7f,
                TopP = 0.8f
            };

            var response = await chat.CompleteChatAsync(messages, options);
            LogTokenUsage(response.Value.Usage);
            return response.Value.Content[0].Text.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting completion from OpenAI for conversation {ConversationId}",
                conversation.Id);
            throw;
        }
    }

    private void LogTokenUsage(ChatTokenUsage usage)
    {
        _logger.LogInformation("Token usage : Input:{InputTokens}, Output:{OutputTokens}, Total: {TotalTokens}",
            usage.InputTokenCount, usage.OutputTokenCount, usage.TotalTokenCount);
    }
}