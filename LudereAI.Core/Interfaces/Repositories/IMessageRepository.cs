using LudereAI.Core.Entities.Chat;

namespace LudereAI.Core.Interfaces.Repositories;

public interface IMessageRepository
{
    Task<IEnumerable<Message>> GetAll();
    Task<IEnumerable<Message>> GetMessages(string conversationId);
    
    Task<bool> Create(Message message);
    Task<bool> CreateBatch(IEnumerable<Message> messages);
    Task<bool> Update(Message message);
    Task<bool> Delete(string messageId);
}