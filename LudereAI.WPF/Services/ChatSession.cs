using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LudereAI.Shared.Enums;
using LudereAI.Shared.Models;
using LudereAI.WPF.Interfaces;
using LudereAI.WPF.Models;

namespace LudereAI.WPF.Services;

public class ChatSession : ObservableObject, IChatSession
{
    private ConversationModel? _currentConversation;
    private bool _isAssistantThinking;

    private bool _isOverrideEnabled;
    private string _manualGameName = string.Empty;
    private WindowInfo? _manualWindow;
    private WindowInfo? _predictedWindow;


    public ObservableCollection<ConversationModel> Conversations { get; } = [];
    public ObservableCollection<MessageModel> Messages { get; } = [];

    public event Action? OnConversationChanged;
    public event Action? OnMessagesChanged;
    public event Action? OnAssistantThinkingChanged;
    public event Action? OnGameContextChanged;

    public ConversationModel? CurrentConversation
    {
        get => _currentConversation;
        set
        {
            if (ReferenceEquals(_currentConversation, value)) return;
            _currentConversation = value;
            OnPropertyChanged();
            UpdateMessages();
            OnConversationChanged?.Invoke();
        }
    }

    public bool IsAssistantThinking
    {
        get => _isAssistantThinking;
        set
        {
            if (_isAssistantThinking == value) return;
            _isAssistantThinking = value;
            OnAssistantThinkingChanged?.Invoke();
        }
    }

    public bool IsOverrideEnabled
    {
        get => _isOverrideEnabled;
        set
        {
            if (_isOverrideEnabled == value) return;
            _isOverrideEnabled = value;
            OnGameContextChanged?.Invoke();
        }
    }

    public string ManualGameName
    {
        get => _manualGameName;
        set
        {
            if (_manualGameName == value) return;
            _manualGameName = value;
            OnGameContextChanged?.Invoke();
        }
    }

    public WindowInfo? ManualWindow
    {
        get => _manualWindow;
        set
        {
            if (ReferenceEquals(_manualWindow, value)) return;
            _manualWindow = value;
            OnGameContextChanged?.Invoke();
        }
    }

    public WindowInfo? PredictedWindow
    {
        get => _predictedWindow;
        set
        {
            if (ReferenceEquals(_predictedWindow, value)) return;
            _predictedWindow = value;
            OnGameContextChanged?.Invoke();
        }
    }

    public string? EffectiveGameContext => IsOverrideEnabled ? ManualGameName : PredictedWindow?.Title;
    public WindowInfo? EffectiveWindow => IsOverrideEnabled ? ManualWindow : PredictedWindow;


    public void SetConversations(ObservableCollection<ConversationModel> newConversations)
    {
        Conversations.Clear();
        foreach (var c in newConversations)
        {
            Conversations.Add(c);
        }

        if (CurrentConversation != null && Conversations.Count > 0)
        {
            CurrentConversation = Conversations[0];
        }
    }

    public void SetMessages(ObservableCollection<MessageModel> newMessages)
    {
        Messages.Clear();
        foreach (var m in newMessages)
        {
            Messages.Add(m);
        }
        OnMessagesChanged?.Invoke();
    }

    public void AddMessageLocal(string content, MessageRole role)
        => AddMessageLocal(new MessageModel{Content = content, Role = role, CreatedAt = DateTime.Now});
    public void AddSystemMessage(string content)
        => AddMessageLocal(content, MessageRole.System);

    public void AddMessageLocal(MessageModel message)
    {
        Messages.Add(message);
        CurrentConversation?.Messages.Add(message);
        OnMessagesChanged?.Invoke();
    }

    private void UpdateMessages()
    {
        Messages.Clear();
        if (_currentConversation != null)
        {
            foreach (var m in _currentConversation.Messages)
            {
                Messages.Add(m);
            }
        }

        OnMessagesChanged?.Invoke();
    }
}