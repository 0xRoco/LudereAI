using LudereAI.Core.Entities;
using LudereAI.Core.Entities.Chat;
using LudereAI.Shared.DTOs;

namespace LudereAI.Core.Interfaces.Services;

public interface IOpenAIService
{
    Task<AIResponse> SendMessage(Conversation conversation, AssistantRequest request); 
    IAsyncEnumerable<AIResponse> StreamlineMessage(Conversation conversation, AssistantRequest request);
    Task<ProcessInfoDTO> PredictGame(List<ProcessInfoDTO> processes);
}