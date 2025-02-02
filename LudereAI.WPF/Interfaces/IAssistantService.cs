using LudereAI.Shared.DTOs;

namespace LudereAI.WPF.Interfaces;

public interface IAssistantService
{
    Task<MessageDTO?> SendMessage(AssistantRequestDTO requestDto);
    Task<IEnumerable<ConversationDTO>> GetConversationsAsync();
}