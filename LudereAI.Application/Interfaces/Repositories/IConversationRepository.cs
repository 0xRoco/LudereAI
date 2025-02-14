using LudereAI.Domain.Models.Chat;

namespace LudereAI.Application.Interfaces.Repositories;

public interface IConversationRepository
{
    Task<Conversation?> Get(string conversationId);
    Task<IEnumerable<Conversation>> GetByAccountId(string accountId);
    Task<bool> Create(Conversation conversation);
    Task<bool> Update(Conversation conversation);
    Task<bool> Delete(string conversationId);
}