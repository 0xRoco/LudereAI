using LudereAI.Core.Entities.Chat;
using LudereAI.Shared;
using LudereAI.Shared.Models;

namespace LudereAI.Core.Interfaces.Services;

public interface IChatService
{
    enum ChatRequestResult
    {
        Success,
        Error
    }
    
    Task<Result<Conversation, ChatRequestResult>> SendMessage(ChatRequest request);
    Task<IEnumerable<Conversation>> GetConversations();
}