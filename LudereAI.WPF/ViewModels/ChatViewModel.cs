using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LudereAI.Core.Interfaces.Services;
using LudereAI.Shared.Models;
using LudereAI.WPF.Interfaces;
using LudereAI.WPF.Models;
using LudereAI.WPF.Services;
using LudereAI.WPF.Views;
using Microsoft.Extensions.Logging;

namespace LudereAI.WPF.ViewModels;

public partial class ChatViewModel : ObservableObject
{
    private readonly ILogger<ChatViewModel> _logger;
    private readonly IChatCoordinator _coordinator;
    private readonly IChatSession _session;

    private readonly INavigationService _navigation;
    private readonly IOverlayService _overlayService;
    private readonly IGameService _gameService;
    private readonly IInputService _inputService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSendMessage))]
    private string _currentMessage = string.Empty;
    [ObservableProperty]
    private bool _isAssistantThinking;

    [ObservableProperty]
    private ObservableCollection<WindowInfo> _windows;
    [ObservableProperty]
    private bool _isRefreshingWindows;

    public bool IsOverrideEnabled
    {
        get => _session.IsOverrideEnabled;
        set
        {
            if (_session.IsOverrideEnabled == value) return;
            _session.IsOverrideEnabled = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanSendMessage));
        }
    }

    public string ManualGameName
    {
        get => _session.ManualGameName;
        set
        {
            if (_session.ManualGameName == value) return;
            _session.ManualGameName = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanSendMessage));
        }
    }

    public WindowInfo? PredicatedWindow
    {
        get => _session.PredictedWindow;
        set
        {
            if (ReferenceEquals(_session.PredictedWindow, value)) return;
            _session.PredictedWindow = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanSendMessage));
        }
    }

    public WindowInfo? ManualWindow
    {
        get => _session.ManualWindow;
        set
        {
            if (ReferenceEquals(_session.ManualWindow, value)) return;
            _session.ManualWindow = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanSendMessage));
        }
    }

    public ConversationModel? CurrentConversation
    {
        get => _session.CurrentConversation;
        set
        {
            if (_session.CurrentConversation == value) return;
            _session.CurrentConversation = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanSendMessage));
        }
    }

    public ObservableCollection<ConversationModel> Conversations => _session.Conversations;

    public bool CanSendMessage => _coordinator.CanSend(CurrentMessage, CurrentConversation, GetGameContext());

    public event Action? OnMessageUpdated;

    public ChatViewModel(
        ILogger<ChatViewModel> logger,
        IChatCoordinator coordinator,
        IChatSession session,
        IOverlayService overlayService,
        IGameService gameService,
        IInputService inputService, INavigationService navigation)
    {
        _logger = logger;
        _coordinator = coordinator;
        _session = session;
        _overlayService = overlayService;
        _gameService = gameService;
        _inputService = inputService;
        _navigation = navigation;

        _session.OnMessagesChanged += () =>
        {
            OnMessageUpdated?.Invoke();
        };
        _session.OnConversationChanged += () =>
        {
            OnPropertyChanged(nameof(CurrentConversation));
            OnPropertyChanged(nameof(CanSendMessage));
        };

        _session.OnAssistantThinkingChanged += () =>
        {
            IsAssistantThinking = _session.IsAssistantThinking;
            OnPropertyChanged(nameof(CanSendMessage));
        };

        Windows = [];

        _gameService.OnGameStarted += w => PredicatedWindow = w;
        _gameService.OnGameStopped += _ => PredicatedWindow = null;

        _ = _gameService.StartScanning();
        _ = _coordinator.Initialize();

        _inputService.OnHotkeyPressed += binding =>
        {
            switch (binding.Id)
            {
                case "ToggleOverlay": _overlayService.ToggleOverlay(); break;
                case "NewChat": NewChat(); break;
            }
        };


        if (CurrentConversation == null)
        {
            NewChat();
        }
    }

    [RelayCommand]
    private void NewChat()
    {
        var convo = new ConversationModel();
        CurrentConversation = convo;
        CurrentMessage = string.Empty;
    }

    [RelayCommand]
    private async Task SendMessage()
    {
        if (!CanSendMessage) return;
        var text = CurrentMessage.Trim();
        CurrentMessage = string.Empty;
        await _coordinator.SendMessage(text, _session.EffectiveGameContext, _session.EffectiveWindow);
        OnPropertyChanged(nameof(CanSendMessage));
    }

    [RelayCommand]
    private async Task LoadConversation(ConversationModel? model)
    {
        if (model == null) return;
        CurrentConversation = model;
        CurrentMessage = string.Empty;
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task RefreshConversations()
    {
        await _coordinator.RefreshConversations();
    }

    [RelayCommand]
    private async Task RefreshProcesses()
    {
        if (IsRefreshingWindows) return;
        IsRefreshingWindows = true;

        try
        {
            var windowInfos = await Task.Run(() =>
            {
                var list = _gameService.GetWindowedProcesses();
                return list.ToList();
            });

            _logger.LogInformation("Refreshed windowed processes, found {Count} windows", windowInfos.Count);
            UpdateWindows(windowInfos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RefreshProcesses failed");
            _session.AddSystemMessage("Failed to refresh windows list.");
        }
        finally
        {
            IsRefreshingWindows = false;
        }
    }

    [RelayCommand]
    private async Task DeleteConversation(ConversationModel? model)
    {
        if (model == null) return;
        await _coordinator.DeleteConversation(model);
    }

    [RelayCommand]
    private async Task PredictGame()
    {
        _logger.LogDebug("Predicting game window...");
        var gameWindow = await _gameService.GetGameWindow();
        if (gameWindow == null)
        {
            _session.AddSystemMessage("No game window detected. Please start a game and try again.");
            PredicatedWindow = null;
            return;
        }

        PredicatedWindow = gameWindow;
    }

    [RelayCommand]
    private void ShowOverlay() => _overlayService.ShowOverlay();

    [RelayCommand]
    private void OpenSettings() => _navigation.ShowWindow<SettingsView>(false, true);

    private string? GetGameContext() => IsOverrideEnabled ? ManualGameName : PredicatedWindow?.Title;

    private void UpdateWindows(IEnumerable<WindowInfo> windows)
    {
        Windows.Clear();
        foreach (var w in windows)
        {
            Windows.Add(w);
        }
    }
}