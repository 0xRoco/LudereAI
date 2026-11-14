using LudereAI.Core.Entities;
using LudereAI.Core.Entities.Chat;
using LudereAI.Shared;
using LudereAI.Shared.DTOs;

namespace LudereAI.Core.Interfaces.Services;

public interface IAIChatService
{
    public enum ConnectionTestResult : byte
    {
        Success = 0,
        Error = 1
    }

    Task<AIResponse> SendMessage(Conversation conversation, AssistantRequest request); 
    IAsyncEnumerable<AIResponse> StreamlineMessage(Conversation conversation, AssistantRequest request);
    Task<ProcessInfoDTO> PredictGame(List<ProcessInfoDTO> processes);
    Task<Result<string, ConnectionTestResult>> TestConnection(AIProvider provider);
}
