using LudereAI.Core.Entities.Chat;

namespace LudereAI.Core.Interfaces.Repositories;

public interface IConversationRepository
{
    Task<Conversation?> Get(string conversationId);
    Task<IEnumerable<Conversation>> GetAll();
    Task<bool> Create(Conversation conversation);
    Task<bool> Update(Conversation conversation);
    Task<bool> Delete(string conversationId);
}