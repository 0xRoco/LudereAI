using LudereAI.Shared.DTOs;
using LudereAI.WPF.Interfaces;
using Microsoft.Extensions.Logging;

namespace LudereAI.WPF.Services;

public class AssistantService(ILogger<IAssistantService> logger, IAPIClient apiClient) : IAssistantService
{
    public async Task<MessageDTO?> SendMessage(AssistantRequestDTO requestDto)
    {
        try
        {
            var result = await apiClient.PostAsync<MessageDTO>("assistant", requestDto);
            
            if (result is { IsSuccess: true, Data: not null })
            {
                return result.Data;
            }
            

            logger.LogWarning("Failed to get response from assistant: {Message}", result?.Message);
            return null;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get response from assistant");
            return null;
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
}