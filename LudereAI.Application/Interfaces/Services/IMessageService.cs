using LudereAI.Shared.DTOs;

namespace LudereAI.Application.Interfaces.Services;

public interface IMessageService
{
    Task<IEnumerable<MessageDTO>> GetMessages(string conversationId);
    Task<MessageDTO?> GetMessage(string conversationId, string messageId);
    
    Task<bool> CreateMessage(string conversationId, MessageDTO dto);
    Task<bool> UpdateMessage(string conversationId, MessageDTO dto);
    Task<bool> DeleteMessage(string conversationId, string messageId);
    
    Task<bool> DeleteMessages(string conversationId);
}