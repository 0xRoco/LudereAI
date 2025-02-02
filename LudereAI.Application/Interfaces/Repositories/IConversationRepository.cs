using LudereAI.Domain;
using LudereAI.Domain.Enums;
using LudereAI.Domain.Models;
using LudereAI.Domain.Models.Chat;
using LudereAI.Shared.Enums;

namespace LudereAI.Application.Interfaces.Repositories;

public interface IConversationRepository
{
    Task<Conversation?> GetConversationAsync(string conversationId);
    Task<IEnumerable<Conversation>> GetConversationsByAccountId(string accountId);
    Task<bool> CreateConversationAsync(Conversation conversation);
}