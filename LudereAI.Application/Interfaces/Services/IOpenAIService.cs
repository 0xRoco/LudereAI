using LudereAI.Domain.Models;
using LudereAI.Domain.Models.Chat;
using LudereAI.Shared.DTOs;

namespace LudereAI.Application.Interfaces.Services;

public interface IOpenAIService
{
    Task<AIResponse> SendMessageAsync(Conversation conversation, AssistantRequestDTO requestDto); 
    IAsyncEnumerable<AIResponse> StreamlineMessageAsync(Conversation conversation, AssistantRequestDTO requestDto);
    Task<ProcessInfoDTO> PredictGame(List<ProcessInfoDTO> processes);
}