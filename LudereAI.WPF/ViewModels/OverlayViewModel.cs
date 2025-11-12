using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LudereAI.WPF.Interfaces;
using LudereAI.WPF.Models;

namespace LudereAI.WPF.ViewModels;

public partial class OverlayViewModel : ObservableObject
{
    private readonly IChatCoordinator _coordinator;
    private readonly IChatSession _session;
    private readonly IOverlayService _overlayService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSendMessage))]
    private string _currentMessage = string.Empty;

    [ObservableProperty]
    private bool _isAssistantThinking;

    public ObservableCollection<ConversationModel> Conversations => _session.Conversations;

    public ConversationModel? CurrentConversation
    {
        get => _session.CurrentConversation;
        private set
        {
            if (_session.CurrentConversation == value) return;
            _session.CurrentConversation = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanSendMessage));
        }
    }
    public bool CanSendMessage => _coordinator.CanSend(CurrentMessage, CurrentConversation, _session.EffectiveGameContext);

    public event Action? OnMessageUpdated;


    public OverlayViewModel(IChatCoordinator coordinator, IChatSession session, IOverlayService overlayService)
    {
        _coordinator = coordinator;
        _session = session;
        _overlayService = overlayService;

        _session.OnMessagesChanged += () =>
        {
            OnMessageUpdated?.Invoke();
        };

        _session.OnAssistantThinkingChanged += () =>
        {
            IsAssistantThinking = _session.IsAssistantThinking;
            OnPropertyChanged(nameof(CanSendMessage));
        };

        _session.OnConversationChanged += () =>
        {
            OnPropertyChanged(nameof(Conversations));
            OnPropertyChanged(nameof(CurrentConversation));
            OnPropertyChanged(nameof(CanSendMessage));
        };

        _session.OnGameContextChanged += () =>
        {
            OnPropertyChanged(nameof(CanSendMessage));
        };
    }

    [RelayCommand]
    private async Task SendMessage()
    {
        if (!CanSendMessage) return;
        var text = CurrentMessage.Trim();
        CurrentMessage = string.Empty;
        await _coordinator.SendMessage(text, CurrentConversation?.GameContext, _session.EffectiveWindow);
        OnPropertyChanged(nameof(CanSendMessage));
    }

    [RelayCommand]
    private void SelectConversation(ConversationModel? convo)
    {
        if (convo != null) CurrentConversation = convo;

        OnPropertyChanged(nameof(CanSendMessage));
    }

    [RelayCommand]
    private void CloseOverlay() => _overlayService.HideOverlay();
}