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
    
    void SetAutoCaptureScreenshots(bool enabled);
    void SetTextToSpeechEnabled(bool enabled);
    
    Task<Result<Message, ChatRequestResult>> SendMessage(ChatRequest request);
    Task<IEnumerable<Conversation>> GetConversations();
}