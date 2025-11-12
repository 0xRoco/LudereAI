using System.Collections.ObjectModel;
using LudereAI.Shared.Enums;
using LudereAI.Shared.Models;
using LudereAI.WPF.Models;

namespace LudereAI.WPF.Interfaces;

public interface IChatSession
{
    ObservableCollection<ConversationModel> Conversations { get; }
    ConversationModel? CurrentConversation { get; set; }
    ObservableCollection<MessageModel> Messages { get; }

    bool IsAssistantThinking { get; set; }

    bool IsOverrideEnabled { get; set; }
    string ManualGameName { get; set; }
    WindowInfo? ManualWindow { get; set; }
    WindowInfo? PredictedWindow { get; set; }

    string? EffectiveGameContext { get; }
    WindowInfo? EffectiveWindow { get; }

    event Action OnConversationChanged;
    event Action OnMessagesChanged;
    event Action OnAssistantThinkingChanged;
    event Action OnGameContextChanged;


    void SetConversations(ObservableCollection<ConversationModel> newConversations);
    void SetMessages(ObservableCollection<MessageModel> newMessages);
    void AddMessageLocal(MessageModel message);
    void AddMessageLocal(string content, MessageRole role);
    public void AddSystemMessage(string content);
}