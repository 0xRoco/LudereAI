using LudereAI.Shared;
using LudereAI.Shared.DTOs;
using LudereAI.WPF.Interfaces;
using Microsoft.Extensions.Logging;

namespace LudereAI.WPF.Services;

public class AssistantService(ILogger<IAssistantService> logger, IAPIClient apiClient) : IAssistantService
{
    
    public enum AssistantRequestResult
    {
        Success,
        Error
    }
    
    public async Task<Result<MessageDTO?, AssistantRequestResult>> SendMessage(AssistantRequestDTO requestDto)
    {
        try
        {
            var result = await apiClient.PostAsync<MessageDTO>("assistant", requestDto);
            
            if (result is { IsSuccess: true, Data: not null })
            {
                return Result<MessageDTO?, AssistantRequestResult>.Success(result.Data);
            }
            

            logger.LogWarning("Failed to get response from assistant: {Message}", result?.Message);
            return Result<MessageDTO?, AssistantRequestResult>.Error(AssistantRequestResult.Error, result?.Message ?? "Failed to get response from assistant");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get response from assistant");
            return Result<MessageDTO?, AssistantRequestResult>.Error(AssistantRequestResult.Error, e.Message);
        }
    }

    public async Task<IEnumerable<ConversationDTO>> GetConversationsAsync()
    {
        try
        {
            var result = await apiClient.GetAsync<IEnumerable<ConversationDTO>>("Conversations/me");
            
            if (result is { IsSuccess: true, Data: not null })
            {
                return result.Data;
            }
            
            logger.LogWarning("Failed to get conversations from assistant: {Message}", result?.Message);
            return Array.Empty<ConversationDTO>();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get conversations from assistant");
            return Array.Empty<ConversationDTO>();
        }
    }

    public async Task<ProcessInfoDTO?> PredictGame(List<ProcessInfoDTO> processes)
    {
        try
        {
            var result = await apiClient.PostAsync<ProcessInfoDTO>("assistant/predictGame", processes);
            
            if (result is { IsSuccess: true, Data: not null})
            {
                return result.Data;
            }
            
            logger.LogWarning("Failed to predict game from assistant: {Message}", result?.Message);
            return null;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to predict game from assistant");
            return null;
        }
    }
}