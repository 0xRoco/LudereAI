using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LudereAI.Shared.DTOs;
using LudereAI.Shared.Enums;
using LudereAI.WPF.Interfaces;
using LudereAI.WPF.Models;
using Microsoft.Extensions.Logging;

namespace LudereAI.WPF.ViewModels;

public partial class ChatViewModel : ObservableObject
{
    private readonly IAssistantService _assistantService;
    private readonly IChatService _chatService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly IAuthService _authService;
    private readonly IScreenshotService _screenshotService;
    private readonly IAudioPlaybackService _audioPlaybackService;
    private readonly ILogger<ChatViewModel> _logger;
    

    public ChatViewModel(
        GameViewModel gameViewModel,
        ILogger<ChatViewModel> logger,
        IScreenshotService screenshotService,
        ISessionService sessionService,
        IAuthService authService, 
        IAudioPlaybackService audioPlaybackService, 
        ISubscriptionService subscriptionService, 
        IChatService chatService, IAssistantService assistantService)
    {
        _logger = logger;
        _screenshotService = screenshotService;
        _authService = authService;
        _audioPlaybackService = audioPlaybackService;
        _subscriptionService = subscriptionService;
        _chatService = chatService;
        _assistantService = assistantService;

        _gameViewModel = gameViewModel;
        
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
        
        //_overlayService.Initialize();
        
        _ =  RefreshConversationsAsync();
    }

    // Observable properties
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanSendMessage), nameof(CanWriteMessage))]
    private bool _isAssistantThinking;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanSendMessage))]
    private string _currentMessage = string.Empty;
    
    [ObservableProperty] private AccountDTO _currentAccount;

    [ObservableProperty] private ObservableCollection<Conversation> _conversations;

    [ObservableProperty] private Conversation? _currentConversation = new();
    
    [ObservableProperty] private ObservableCollection<WindowInfo> _windows;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanSendMessage))]
    private WindowInfo? _selectedWindow;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanSendMessage))]
    private WindowInfo? _predicatedWindow;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanSendMessage))]
    private bool _isOverrideEnabled;


    [ObservableProperty] private GameViewModel _gameViewModel;
    
    public event Action OnMessageUpdated;

    public bool CanSendMessage =>
        !string.IsNullOrWhiteSpace(CurrentMessage) && !IsAssistantThinking && (IsOverrideEnabled ? SelectedWindow != null : PredicatedWindow != null);

    public bool CanWriteMessage => !IsAssistantThinking;
    public bool CanShowSubscriptionOptions => CurrentAccount is { IsSubscribed: false, Tier: not SubscriptionTier.Guest};

    [RelayCommand]
    private void NewChat()
    {
        CurrentConversation = new Conversation();
        CurrentMessage = string.Empty;
        OnMessageUpdated.Invoke();
    }

    [RelayCommand]
    private async Task SendMessage()
    {
        if (!CanSendMessage) return;
        if (!string.IsNullOrWhiteSpace(CurrentConversation?.GameContext) && CurrentConversation?.GameContext != (IsOverrideEnabled ? GameViewModel.CurrentGame : PredicatedWindow?.Title))
        {
            AddSystemMessage("An active conversation is limited to only a single game, please start a new conversation.");
            return;
        }

        var message = CurrentMessage.Trim();
        CurrentMessage = string.Empty;
        
        await ProcessMessage(message);
    }

    [RelayCommand]
    private void RefreshProcesses() => Windows = new ObservableCollection<WindowInfo>(_screenshotService.GetWindowedProcessesAsync());
    
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
            PredicatedWindow = Windows.FirstOrDefault(w => w.ProcessId == predicatedProcess.ProcessId);
        }
    }

    [RelayCommand]
    private async Task Logout() => await _authService.LogoutAsync();

    [RelayCommand]
    private void ManageSubscription() => Process.Start (new ProcessStartInfo("https://staging.LudereAI.com/Account") { UseShellExecute = true });


    [RelayCommand]
    private async Task SubscribeProMonthly()
    {
        await _subscriptionService.Subscribe(SubscriptionPlan.Pro);
        
        MessageBox.Show("You will now be logged out to apply the changes.", "Subscription Updated", MessageBoxButton.OK, MessageBoxImage.Information); 
        await _authService.LogoutAsync();
    }
    
    [RelayCommand]
    private async Task SubscribeProYearly()
    {
        await _subscriptionService.Subscribe(SubscriptionPlan.ProYearly);
        
        MessageBox.Show("You will now be logged out to apply the changes.", "Subscription Updated", MessageBoxButton.OK, MessageBoxImage.Information);
        await _authService.LogoutAsync();
    }
    
    [RelayCommand]
    private async Task SubscribeUltimateMonthly()
    {
        await _subscriptionService.Subscribe(SubscriptionPlan.Ultimate);
        
        MessageBox.Show("You will now be logged out to apply the changes.", "Subscription Updated", MessageBoxButton.OK, MessageBoxImage.Information);
        await _authService.LogoutAsync();
    }
    
    [RelayCommand]
    private async Task SubscribeUltimateYearly()
    {
        await _subscriptionService.Subscribe(SubscriptionPlan.UltimateYearly);
        
        MessageBox.Show("You will now be logged out to apply the changes.", "Subscription Updated", MessageBoxButton.OK, MessageBoxImage.Information);
        await _authService.LogoutAsync();
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
                GameContext = IsOverrideEnabled ? GameViewModel.CurrentGame : PredicatedWindow?.Title,
                Window = IsOverrideEnabled ? SelectedWindow : PredicatedWindow
            });
            
            

            if (result is { IsSuccessful: true, Value: not null })
            {
                var response = result.Value;
                
                await RefreshConversationsAsync();
                CurrentConversation = Conversations.FirstOrDefault(c => c.Id == response.ConversationId);
                if (CurrentConversation != null)
                {
                   //Nothing
                }
                else
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
            AddSystemMessage("An error occurred while processing your request.");
        }finally
        {
            IsAssistantThinking = false;
        }
    }

    
    private async Task RefreshConversationsAsync()
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
        OnMessageUpdated.Invoke();
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
}