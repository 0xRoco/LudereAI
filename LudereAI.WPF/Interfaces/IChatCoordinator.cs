using LudereAI.Shared.Models;
using LudereAI.WPF.Models;

namespace LudereAI.WPF.Interfaces;

public interface IChatCoordinator
{
    bool CanSend(string message, ConversationModel? conversation, string? gameContext);

    Task Initialize();
    Task RefreshConversations();
    Task DeleteConversation(ConversationModel conversation);
    Task SendMessage(string message, string? gameContext, WindowInfo? windowContext);
}