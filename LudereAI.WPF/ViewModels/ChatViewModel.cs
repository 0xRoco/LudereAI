using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LudereAI.Shared.DTOs;
using LudereAI.Shared.Enums;
using LudereAI.WPF.Interfaces;
using LudereAI.WPF.Models;
using LudereAI.WPF.Views;
using Microsoft.Extensions.Logging;

namespace LudereAI.WPF.ViewModels;

public partial class ChatViewModel : ObservableObject
{
    private readonly ILogger<ChatViewModel> _logger;
    private readonly IAssistantService _assistantService;
    private readonly IChatService _chatService;
    private readonly IAuthService _authService;
    private readonly IAudioPlaybackService _audioPlaybackService;
    private readonly INavigationService _navigationService;
    private readonly IOverlayService _overlayService;
    private readonly IGameService _gameService;
    private readonly IInputService _inputService;
    
    private event Action<Conversation> OnConversationChanged; 
    public event Action OnMessageUpdated;
    

    public ChatViewModel(
        ILogger<ChatViewModel> logger,
        ISessionService sessionService,
        IAuthService authService, 
        IAudioPlaybackService audioPlaybackService, 
        IChatService chatService, 
        IAssistantService assistantService, 
        INavigationService navigationService, 
        IGameService gameService, IOverlayService overlayService, IInputService inputService)
    {
        _logger = logger;
        _authService = authService;
        _audioPlaybackService = audioPlaybackService;
        _chatService = chatService;
        _assistantService = assistantService;
        _navigationService = navigationService;
        _gameService = gameService;
        _overlayService = overlayService;
        _inputService = inputService;

        if (sessionService.CurrentAccount != null)
        {
            CurrentAccount = sessionService.CurrentAccount;
        }
        else
        { 
            _authService.LogoutAsync();
        }
        
        
        Conversations = [];
        Windows = [];
        
        _gameService.OnGameStarted += OnGameStarted;
        _gameService.OnGameStopped += OnGameStopped;
        
        OnConversationChanged += c =>
        {
            Messages = new ObservableCollection<Message>(c.Messages);
            OnMessageUpdated?.Invoke();
        };

        _ = RefreshConversations();
        
        _ = _gameService.StartScanning();
        
        Init();
    }

    public void Init()
    {
        
        _inputService.Start();

        _inputService.OnHotkeyPressed += binding =>
        {
            switch (binding.Id)
            {
                case "ToggleOverlay":  _overlayService.ToggleOverlay(); break;
                case "NewChat": NewChat(); break;
            }
        };
    }

    // Observable properties
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanSendMessage), nameof(CanWriteMessage))]
    private bool _isAssistantThinking;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanSendMessage))]
    private string _currentMessage = string.Empty;
    
    [ObservableProperty] private AccountDTO _currentAccount;

    [ObservableProperty] private ObservableCollection<Conversation> _conversations;
    [ObservableProperty] private ObservableCollection<Message> _messages;

    [ObservableProperty] private Conversation? _currentConversation = new();
    
    [ObservableProperty] private ObservableCollection<WindowInfo> _windows;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanSendMessage))]
    private WindowInfo? _manualWindow;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanSendMessage))]
    private string _manualGameName = string.Empty;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanSendMessage))]
    private WindowInfo? _predicatedWindow;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanSendMessage))]
    private bool _isOverrideEnabled;
    
    public bool CanSendMessage =>
        !string.IsNullOrWhiteSpace(CurrentMessage) && !IsAssistantThinking && (IsOverrideEnabled ? ManualWindow != null : PredicatedWindow != null);

    public bool CanWriteMessage => !IsAssistantThinking;
    public bool CanShowSubscriptionOptions => CurrentAccount is { IsSubscribed: true, Tier: not SubscriptionTier.Guest};

    [RelayCommand]
    private void NewChat()
    {
        CurrentConversation = new Conversation();
        CurrentMessage = string.Empty;
    }

    [RelayCommand]
    private async Task SendMessage()
    {
        if (!CanSendMessage) return;
        if (!string.IsNullOrWhiteSpace(CurrentConversation?.GameContext) && CurrentConversation?.GameContext != (IsOverrideEnabled ? ManualGameName : PredicatedWindow?.Title))
        {
            AddSystemMessage("An active conversation is limited to only a single game, please start a new conversation.");
            return;
        }

        var message = CurrentMessage.Trim();
        CurrentMessage = string.Empty;
        
        await ProcessMessage(message);
    }

    [RelayCommand]
    private void RefreshProcesses() => Windows = new ObservableCollection<WindowInfo>(_gameService.GetWindowedProcesses());
    
    [RelayCommand]
    private async Task PredictGame()
    {
        RefreshProcesses();
        
        var processes = Windows.Select(w => new ProcessInfoDTO
        {
            ProcessId = w.ProcessId,
            ProcessName= w.ProcessName,
            Title = w.Title
        }).ToList();
        
        var predicatedProcess = await _assistantService.PredictGame(processes);
        
        if (predicatedProcess != null)
        {
            var process = Windows.FirstOrDefault(w => w.ProcessId == predicatedProcess.ProcessId);
            if (process != null)
            {
                process.Title = predicatedProcess.Title;

                PredicatedWindow = process;
                return;
            }
        }
        
        PredicatedWindow = null;
    }

    [RelayCommand]
    private async Task Logout() => await _authService.LogoutAsync();

    [RelayCommand]
    private void ManageSubscription() => Process.Start (new ProcessStartInfo("https://staging.LudereAI.com/Account") { UseShellExecute = true });

    
    [RelayCommand]
    private void ShowOverlay() => _overlayService.ShowOverlay();

    [RelayCommand]
    private void CloseOverlay() => _overlayService.HideOverlay();

    [RelayCommand]
    private void OpenSettings()
    {
        _navigationService.ShowWindow<SettingsView>(false, true);
    }
    
    private async Task ProcessMessage(string message)
    {
        try
        {
            IsAssistantThinking = true;
            AddMessage(message, MessageRole.User);

            var result = await _chatService.SendMessage(new ChatRequest
            {
                Message = message,
                ConversationId = CurrentConversation?.Id,
                GameContext = IsOverrideEnabled ? ManualGameName : PredicatedWindow?.Title,
                Window = IsOverrideEnabled ? ManualWindow : PredicatedWindow
            });
            
            

            if (result is { IsSuccessful: true, Value: not null })
            {
                var response = result.Value;
                
                await RefreshConversations();
                CurrentConversation = Conversations.FirstOrDefault(c => c.Id == response.ConversationId);
                if (CurrentConversation == null)
                {
                    AddMessage(response.Content, MessageRole.Assistant);
                }
                
                await _audioPlaybackService.PlayAudioAsync(response.Audio);
            }
            else
            {
                AddSystemMessage(result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process message");
            
            if (ex.Message.Contains("invalid window dimensions", StringComparison.CurrentCultureIgnoreCase))
            {
                AddSystemMessage("The window dimensions are invalid, please select a valid window.");
                PredicatedWindow = null;
                ManualWindow = null;
                return;
            }
            
            AddSystemMessage("An error occurred while processing your request.");
        }finally
        {
            IsAssistantThinking = false;
        }
    }

    
    private async Task RefreshConversations()
    {
        try
        {
            var conversationsAsync = await _chatService.GetConversations();
            Conversations = new ObservableCollection<Conversation>(conversationsAsync);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh conversations.");
        }
    }

    partial void OnCurrentConversationChanged(Conversation? value)
    {
        if (value == null) return;
        
        CurrentConversation = value;
        OnConversationChanged.Invoke(CurrentConversation);
    }

    partial void OnConversationsChanged(ObservableCollection<Conversation> value)
    {
        if (CurrentConversation == null && value.Count > 0)
        {
            CurrentConversation = value.First();
        }
    }

    private void AddMessage(string content, MessageRole role) =>
        CurrentConversation?.AddMessage(new Message { Content = content, Role = role });

    private void AddSystemMessage(string content) =>
        AddMessage(content, MessageRole.System);
    
    private void OnGameStarted(WindowInfo gameWindow)
    {
        PredicatedWindow = gameWindow;
    }

    private void OnGameStopped(WindowInfo gameWindow)
    {
        PredicatedWindow = null;
    }
}