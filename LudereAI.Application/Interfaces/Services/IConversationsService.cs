using LudereAI.Shared.DTOs;

namespace LudereAI.Application.Interfaces.Services;

public interface IConversationsService
{
    Task<IEnumerable<ConversationDTO>> GetConversations();
    Task<IEnumerable<ConversationDTO>> GetConversationsByUser(string userId);
    Task<ConversationDTO?> GetConversation(string id);
    
    Task<bool> CreateConversation(ConversationDTO dto);
    Task<bool> UpdateConversation(ConversationDTO dto);
    Task<bool> DeleteConversation(string id);
}