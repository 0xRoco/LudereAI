using LudereAI.Domain.Models.Chat;

namespace LudereAI.Application.Interfaces.Repositories;

public interface IConversationRepository
{
    Task<Conversation?> GetConversationAsync(string conversationId);
    Task<IEnumerable<Conversation>> GetConversationsByAccountId(string accountId);
    Task<bool> CreateConversationAsync(Conversation conversation);
}