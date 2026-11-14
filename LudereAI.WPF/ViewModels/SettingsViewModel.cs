using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LudereAI.Core.Entities;
using LudereAI.Core.Interfaces.Services;
using LudereAI.Services;
using LudereAI.WPF.Interfaces;
using LudereAI.WPF.Views;
using Microsoft.Extensions.Logging;

namespace LudereAI.WPF.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ILogger<SettingsViewModel> _logger;
    private readonly IAIChatService _aiChatService;
    private readonly ISettingsService _settingsService;
    private readonly IInputService _inputService;
    private readonly INavigationService _navigationService;
    
    [ObservableProperty]
    private AppSettings _appSettings = new();
    
    [ObservableProperty]
    private AIProvider _selectedAIProviderTemplate = new();
    
    [ObservableProperty]
    private TTSProvider? _selectedTtsProviderTemplate;

    [ObservableProperty]
    private bool _isTestingAIProvider;

    [ObservableProperty]
    private string _aiProviderTestMessage = string.Empty;

    [ObservableProperty]
    private bool _aiProviderTestSucceeded;

    [ObservableProperty] private ObservableCollection<KeyBindingItemViewModel> _keyBindings = [];
    
    public ObservableCollection<AIProvider> AIProviderTemplates { get; } =
        new(AIProviderFactory.GetAvailableProviders());
    
    public ObservableCollection<TTSProvider> TTSProviderTemplates { get; } =
        new(TTSProviderFactory.GetAvailableProviders());


    public List<string> Themes { get; } = ["Light", "Dark", "System"];
    public List<string> Languages { get; } = ["English"];
    
    public SettingsViewModel(ILogger<SettingsViewModel> logger, IAIChatService aiChatService, ISettingsService settingsService, IInputService inputService, INavigationService navigationService)
    {
        _logger = logger;
        _aiChatService = aiChatService;
        _settingsService = settingsService;
        _inputService = inputService;
        _navigationService = navigationService;

        _ = Load();
    }

    [RelayCommand]
    private async Task Save()
    {
        AppSettings.KeyBind.Hotkeys = KeyBindings.Select(kb => kb.ToKeyBinding()).ToList();
        
        await _settingsService.SaveSettings(AppSettings, CancellationToken.None);
        _settingsService.ApplySettings(AppSettings);
        
        _navigationService.CloseWindow<SettingsView>();
    }

    [RelayCommand]
    private async Task TestAIProviderConnection()
    {
        if (IsTestingAIProvider) return;
        if (AppSettings?.General?.AIProvider == null) return;

        if (string.IsNullOrWhiteSpace(AppSettings.General.AIProvider.ApiKey)
            || string.IsNullOrWhiteSpace(AppSettings.General.AIProvider.BaseUrl)
            || string.IsNullOrWhiteSpace(AppSettings.General.AIProvider.Model))
        {
            AiProviderTestSucceeded = false;
            AiProviderTestMessage = "API key, base URL, and model are required.";
            return;
        }

        IsTestingAIProvider = true;
        AiProviderTestMessage = "Testing provider connection...";
        AiProviderTestSucceeded = false;

        try
        {
            var result = await _aiChatService.TestConnection(AppSettings.General.AIProvider);

            AiProviderTestSucceeded = result.IsSuccessful;
            AiProviderTestMessage = string.IsNullOrWhiteSpace(result.Message)
                ? (result.IsSuccessful ? "Connection succeeded." : "Connection failed.")
                : result.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI provider connection test failed unexpectedly");
            AiProviderTestSucceeded = false;
            AiProviderTestMessage = ex.Message;
        }
        finally
        {
            IsTestingAIProvider = false;
        }
    }

    [RelayCommand]
    private async Task Cancel()
    {
        await Load();
        _navigationService.CloseWindow<SettingsView>();
    }

    [RelayCommand]
    private async Task Load() 
    {
        AppSettings = await _settingsService.LoadSettings();
        SelectedAIProviderTemplate = AIProviderTemplates.FirstOrDefault(p => p.ProviderType == AppSettings.General.AIProvider?.ProviderType) 
                             ?? AIProviderTemplates.First();
        
        SelectedTtsProviderTemplate = TTSProviderTemplates.FirstOrDefault(p => p.ProviderType == AppSettings.General.TTSProvider?.ProviderType) 
                             ?? TTSProviderTemplates.First();
        LoadKeyBindings();
        IsTestingAIProvider = false;
        AiProviderTestMessage = string.Empty;
        AiProviderTestSucceeded = false;
    }
    
    private void LoadKeyBindings()
    {
        KeyBindings.Clear();
        
        foreach (var keyBinding in AppSettings.KeyBind.Hotkeys)
        {
            KeyBindings.Add(new KeyBindingItemViewModel(keyBinding, _inputService));
        }
    }

    partial void OnSelectedAIProviderTemplateChanged(AIProvider value)
    {
        var currentProvider = AppSettings.General.AIProvider;

        AppSettings.General.AIProvider = new AIProvider
        {
            ProviderType = value.ProviderType,
            BaseUrl = string.IsNullOrWhiteSpace(currentProvider.BaseUrl) ? value.BaseUrl : currentProvider.BaseUrl,
            Model = string.IsNullOrWhiteSpace(currentProvider.Model) ? value.Model : currentProvider.Model,
            ApiKey = currentProvider.ApiKey
        };

        OnPropertyChanged(nameof(AppSettings));
        AiProviderTestMessage = string.Empty;
        AiProviderTestSucceeded = false;
    }

    partial void OnSelectedTtsProviderTemplateChanged(TTSProvider? value)
    {
        if (value == null) return;

        var currentConfig = AppSettings.General.TTSProvider;
        
        AppSettings.General.TTSProvider = new TTSProvider
        {
            ProviderType = value.ProviderType,
            ApiKey = currentConfig.ApiKey, 
            VoiceId = value.RequiresVoice ? currentConfig.VoiceId : value.VoiceId,
            BaseUrl = value.RequiresBaseUrl ? value.BaseUrl : currentConfig.BaseUrl
        };
        
        OnPropertyChanged(nameof(AppSettings));
    }
}
