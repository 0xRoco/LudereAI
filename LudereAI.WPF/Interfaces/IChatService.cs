using LudereAI.Shared;
using LudereAI.Shared.DTOs;
using LudereAI.WPF.Models;

namespace LudereAI.WPF.Interfaces;

public interface IChatService
{
    enum ChatRequestResult
    {
        Success,
        Error
    }
    Task<Result<MessageDTO, ChatRequestResult>> SendMessage(ChatRequest request);
    Task<IEnumerable<ConversationDTO>> GetConversations();
}