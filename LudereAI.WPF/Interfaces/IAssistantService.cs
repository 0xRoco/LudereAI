using LudereAI.Shared;
using LudereAI.Shared.DTOs;
using LudereAI.WPF.Services;

namespace LudereAI.WPF.Interfaces;

public interface IAssistantService
{
    Task<Result<MessageDTO?, AssistantService.AssistantRequestResult>> SendMessage(AssistantRequestDTO requestDto);
    Task<IEnumerable<ConversationDTO>> GetConversationsAsync();
}