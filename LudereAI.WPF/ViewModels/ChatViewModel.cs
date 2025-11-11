using System.Collections.ObjectModel;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LudereAI.Core.Entities.Chat;
using LudereAI.Core.Interfaces.Services;
using LudereAI.Shared.DTOs;
using LudereAI.Shared.Enums;
using LudereAI.Shared.Models;
using LudereAI.WPF.Interfaces;
using LudereAI.WPF.Models;
using LudereAI.WPF.Views;
using Microsoft.Extensions.Logging;

namespace LudereAI.WPF.ViewModels;

public partial class ChatViewModel : ObservableObject
{
    private readonly ILogger<ChatViewModel> _logger;
    private readonly IMapper _mapper;
    private readonly IChatService _chatService;
    private readonly IAudioService _audioService;
    private readonly INavigationService _navigationService;
    private readonly IOverlayService _overlayService;
    private readonly IGameService _gameService;
    private readonly IInputService _inputService;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanSendMessage), nameof(CanWriteMessage))]
    private bool _isAssistantThinking;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanSendMessage))]
    private string _currentMessage = string.Empty;
    [ObservableProperty] private ObservableCollection<ConversationModel> _conversations;
    [ObservableProperty] private ObservableCollection<MessageModel> _messages;
    [ObservableProperty] private ConversationModel? _currentConversation = new();
    [ObservableProperty] private ObservableCollection<WindowInfo> _windows;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanSendMessage))]
    private WindowInfo? _manualWindow;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanSendMessage))]
    private string _manualGameName = string.Empty;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanSendMessage))]
    private WindowInfo? _predicatedWindow;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanSendMessage))]
    private bool _isOverrideEnabled;
    [ObservableProperty] private bool _isRefreshingWindows;
    
    public bool CanSendMessage => !string.IsNullOrWhiteSpace(CurrentMessage) && !IsAssistantThinking && (IsOverrideEnabled ? ManualWindow != null : PredicatedWindow != null);
    public bool CanWriteMessage => !IsAssistantThinking;
    public event Action OnMessageUpdated;
    
    public ChatViewModel(
        ILogger<ChatViewModel> logger,
        IAudioService audioService, 
        IChatService chatService, 
        INavigationService navigationService, 
        IGameService gameService, IOverlayService overlayService, IInputService inputService, IMapper mapper)
    {
        _logger = logger;
        _audioService = audioService;
        _chatService = chatService;
        _navigationService = navigationService;
        _gameService = gameService;
        _overlayService = overlayService;
        _inputService = inputService;
        _mapper = mapper;

        Conversations = [];
        Windows = [];
        
        _gameService.OnGameStarted += OnGameStarted;
        _gameService.OnGameStopped += OnGameStopped;


        _ = RefreshConversations();
        _ = _gameService.StartScanning();
        
        Init();
    }

    private void Init()
    {
        _inputService.OnHotkeyPressed += binding =>
        {
            switch (binding.Id)
            {
                case "ToggleOverlay":  _overlayService.ToggleOverlay(); break;
                case "NewChat": NewChat(); break;
            }
        };
    }

    [RelayCommand]
    private void NewChat()
    {
        CurrentConversation = new ConversationModel();
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
    private async Task RefreshProcesses()
    {
        if (IsRefreshingWindows) return;
        IsRefreshingWindows = true;

        try
        {
            var windowInfos = await Task.Run(() =>
            {
                var list = _gameService.GetWindowedProcesses();
                return list?.ToList() ?? [];
            });

            _logger.LogInformation("Refreshed windowed processes, found {Count} windows", windowInfos.Count);
            UpdateWindows(windowInfos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RefreshProcesses failed");
            AddSystemMessage("Failed to refresh windows list.");
        }
        finally
        {
            IsRefreshingWindows = false;
        }
    }
    
    [RelayCommand]
    private async Task PredictGame()
    {
        _logger.LogDebug("Predicting game window...");
        var gameWindow = await _gameService.GetGameWindow();
        if (gameWindow == null)
        {
            AddSystemMessage("No game window detected. Please start a game and try again.");
            PredicatedWindow = null;
            return;
        }
        
        PredicatedWindow = gameWindow;
    }

    [RelayCommand]
    private async Task LoadConversation(ConversationModel? model)
    {
        if (model == null) return;

        try
        {
            if (Conversations.All(c => c.Id != model.Id)) await RefreshConversations();

            var item = Conversations.FirstOrDefault(c => c.Id == model.Id) ?? model;

            item.Messages.ToList().ForEach(m => m.CreatedAt = m.CreatedAt.ToLocalTime());

            CurrentConversation = item;

            CurrentMessage = string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load conversation {ConversationId}", model.Id);
            AddSystemMessage("Failed to load conversation.");
        }
    }

    [RelayCommand]
    private async Task DeleteConversation(ConversationModel? model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.Id)) return;

        try
        {
            var result = true;
            if (result)
            {
                Conversations.Remove(model);
                if (CurrentConversation?.Id == model.Id)
                {
                    CurrentConversation = Conversations.FirstOrDefault();
                }
            }
            else
            {
                AddSystemMessage("Failed to delete conversation: " + result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete conversation {ConversationId}", model.Id);
            AddSystemMessage("An error occurred while deleting the conversation.");
        }
    }
    
    
    [RelayCommand]
    private void ShowOverlay() => _overlayService.ShowOverlay();

    [RelayCommand]
    private void CloseOverlay() => _overlayService.HideOverlay();

    [RelayCommand]
    private void OpenSettings()
    {
        _navigationService.ShowWindow<SettingsView>(false, true);
    }

    private void UpdateWindows(IEnumerable<WindowInfo> windows)
    {
        Windows.Clear();
        foreach (var w in windows)
        {
            Windows.Add(w);
        }
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
                var conversation = result.Value;
                
                UpdateConversationState(conversation);

                var lastMessage = CurrentConversation?.Messages.LastOrDefault(m => m.Role == MessageRole.Assistant);
                
                if (lastMessage?.Audio.Length > 0)
                    await _audioService.PlayAudioAsync(conversation.Messages.Last().Audio);
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
            OnMessageUpdated?.Invoke();
        }
    }
    
    private async Task RefreshConversations()
    {
        try
        {
            var conversationEntities = await _chatService.GetConversations();
            var uiConversations = _mapper.Map<IEnumerable<ConversationModel>>(conversationEntities);
            Conversations = new ObservableCollection<ConversationModel>(uiConversations);

            if (CurrentConversation == null || string.IsNullOrWhiteSpace(CurrentConversation.Id))
            {
                CurrentConversation = Conversations.FirstOrDefault();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh conversations.");
        }
    }
    
    private void UpdateConversationState(Conversation conversation)
    {
        var conversationModel = _mapper.Map<ConversationModel>(conversation);
        var existingConversation = Conversations.FirstOrDefault(c => c.Id == conversationModel.Id);
        if (existingConversation != null)
        {
            var index = Conversations.IndexOf(existingConversation);
            Conversations[index] = conversationModel;
        }
        else
        {
            Conversations.Insert(0, conversationModel);
        }
        
        conversationModel.Messages.ToList().ForEach(m => m.CreatedAt = m.CreatedAt.ToLocalTime());
        CurrentConversation = conversationModel;
    }
    
    private void AddMessage(string content, MessageRole role) => AddMessage(new MessageModel { Content = content, Role = role });

    private void AddMessage(MessageModel message)
    {
        Messages.Add(message);
        OnMessageUpdated?.Invoke();
    }
    
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
    
    partial void OnCurrentConversationChanged(ConversationModel? value)
    {
        if (value == null)
        {
            Messages = [];
        }
        else
        {
            value.Messages.ToList().ForEach(m => m.CreatedAt = m.CreatedAt.ToLocalTime());
            Messages = new ObservableCollection<MessageModel>(value.Messages);
        }
        
        OnMessageUpdated?.Invoke();
    }

    partial void OnConversationsChanged(ObservableCollection<ConversationModel> value)
    {
        if (CurrentConversation == null && value.Count > 0)
        {
            CurrentConversation = value.First();
        }
    }

    partial void OnIsOverrideEnabledChanged(bool value)
    {
        if (value && Windows.Count == 0)
        {
            _ = RefreshProcesses();
        }
    }
}