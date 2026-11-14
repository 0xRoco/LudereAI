using LudereAI.Core.Entities;
using LudereAI.Core.Entities.Chat;
using LudereAI.Core.Interfaces;
using LudereAI.Core.Interfaces.Services;
using LudereAI.Shared;
using LudereAI.Shared.DTOs;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

#pragma warning disable OPENAI001

namespace LudereAI.Services.Services;

public class AIChatService(
    ILogger<IAIChatService> logger,
    IChatClientFactory chatClientFactory,
    ITextToSpeechService textToSpeechService,
    IMessageHandler messageHandler)
    : IAIChatService
{
    public async Task<Result<string, IAIChatService.ConnectionTestResult>> TestConnection(AIProvider provider)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(provider);

            if (string.IsNullOrWhiteSpace(provider.BaseUrl))
                return Result<string, IAIChatService.ConnectionTestResult>.Error(IAIChatService.ConnectionTestResult.Error, "Base URL is required.");

            if (string.IsNullOrWhiteSpace(provider.Model))
                return Result<string, IAIChatService.ConnectionTestResult>.Error(IAIChatService.ConnectionTestResult.Error, "Model is required.");

            var client = new OpenAIClient(new ApiKeyCredential(provider.ApiKey), new OpenAIClientOptions
            {
                Endpoint = new Uri(provider.BaseUrl)
            });

            var chat = client.GetChatClient(provider.Model);
            var response = await chat.CompleteChatAsync([
                new SystemChatMessage("You are a health check endpoint."),
                new UserChatMessage("reply with pong")
            ], new ChatCompletionOptions
            {
                MaxOutputTokenCount = 16,
                Temperature = 0
            });

            if (response.Value.Content.Count == 0)
                return Result<string, IAIChatService.ConnectionTestResult>.Error(IAIChatService.ConnectionTestResult.Error, "Provider responded with empty content.");

            return Result<string, IAIChatService.ConnectionTestResult>.Success($"Connected. Provider responded: {response.Value.Content[0].Text.Trim()}");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "AI provider connection test failed");
            return Result<string, IAIChatService.ConnectionTestResult>.Error(IAIChatService.ConnectionTestResult.Error, ex.Message);
        }
    }

    public async Task<AIResponse> SendMessage(Conversation conversation, AssistantRequest request)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(conversation);
            ArgumentNullException.ThrowIfNull(request);

            /*var file = await _messageHandler.HandleScreenshot(request, conversation);
            if (file != null && !string.IsNullOrWhiteSpace(file.Url))
            {
                request.Screenshot = file.Url;
            }*/
            

            var aiMessage = await ProcessRequest(conversation, request);

            byte[] audio = [];

            if (request.TextToSpeechEnabled)
            {
                //BUG: This increases latency by shitton
                audio = await textToSpeechService.GenerateTTS(aiMessage);
            }

            var aiResponse = new AIResponse()
            {
                MessageId = Guid.NewGuid().ToString("N"),
                ConversationId = conversation.Id,
                Message = aiMessage,
                TextToSpeech = audio
            };

            await messageHandler.SaveConversationMessages(request, aiResponse);

            return aiResponse;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing message for conversation {ConversationId}", conversation.Id);
            throw;
        }
    }
    
    public async IAsyncEnumerable<AIResponse> StreamlineMessage(Conversation conversation, AssistantRequest request)
    {
        ArgumentNullException.ThrowIfNull(conversation);
        ArgumentNullException.ThrowIfNull(request);
        
        /*var file = await _messageHandler.HandleScreenshot(request, conversation);
        if (file != null && !string.IsNullOrWhiteSpace(file.Url))
        {
            request.Screenshot = file.Url;
        }*/
        
        
        var messages = await messageHandler.BuildMessageHistory(request, conversation);
        var chat = chatClientFactory.CreateChatClient();
        var options = new ChatCompletionOptions
        {
            MaxOutputTokenCount = 1048,
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
        
        var chat = chatClientFactory.CreateChatClient();
        
        var response = await chat.CompleteChatAsync(messages);
        LogTokenUsage(response.Value.Usage);
        
        logger.LogInformation("Predicted game: {Prediction}", response.Value.Content[0].Text.Trim());
        
        return response.Value.Content[0].Text.Trim().FromJson<ProcessInfoDTO>() ?? new ProcessInfoDTO();
    }

    private async Task<string> ProcessRequest(Conversation conversation, AssistantRequest request)
    {
        var messages = await messageHandler.BuildMessageHistory(request, conversation);
        var chat = chatClientFactory.CreateChatClient();

        try
        {
            var options = new ChatCompletionOptions
            {
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
            logger.LogError(ex, "Error getting completion from OpenAI for conversation {ConversationId}",
                conversation.Id);
            throw;
        }
    }

    private void LogTokenUsage(ChatTokenUsage usage)
    {
        logger.LogInformation("Token usage : Input:{InputTokens}, Output:{OutputTokens}, Total: {TotalTokens}",
            usage.InputTokenCount, usage.OutputTokenCount, usage.TotalTokenCount);
    }
}
