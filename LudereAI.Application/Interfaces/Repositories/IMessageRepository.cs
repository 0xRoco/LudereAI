using LudereAI.Domain.Models;
using LudereAI.Domain.Models.Chat;

namespace LudereAI.Application.Interfaces.Repositories;

public interface IMessageRepository
{
    Task<IEnumerable<Message>> GetAllAsync();
    Task<IEnumerable<Message>> GetMessagesAsync(string conversationId);
    
    Task<bool> CreateAsync(Message message);
    Task<bool> UpdateAsync(Message message);
    Task<bool> DeleteAsync(string messageId);
}