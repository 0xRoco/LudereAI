using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LudereAI.Core.Entities;
using LudereAI.Core.Interfaces.Services;
using LudereAI.Services;
using LudereAI.Shared.Enums;
using LudereAI.Shared.Models;
using LudereAI.WPF.Interfaces;
using LudereAI.WPF.Views;

namespace LudereAI.WPF.ViewModels;

public partial class SetupViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly INavigationService _navigationService;
    
    [ObservableProperty] 
    private AppSettings _appSettings = new();
    
    [ObservableProperty] 
    private AIProvider _selectedAIProviderTemplate = new();
    [ObservableProperty]
    private TTSProvider? _selectedTtsProviderTemplate;
    
    public ObservableCollection<AIProvider> AIProviderTemplates { get; } = new(AIProviderFactory.GetAvailableProviders());
    public ObservableCollection<TTSProvider> TtsProviderTemplates { get; } =
        new(TTSProviderFactory.GetAvailableProviders());


    public SetupViewModel(ISettingsService settingsService, INavigationService navigationService)
    {
        _settingsService = settingsService;
        _navigationService = navigationService;

        SelectedAIProviderTemplate = AIProviderTemplates.First(p => p.ProviderType == AIProviderType.OpenAI);
        SelectedTtsProviderTemplate = TtsProviderTemplates.First(p => p.ProviderType == TTSProviderType.Windows);
    }

    [RelayCommand]
    private async Task SaveAndContinue()
    {
        AppSettings.Advanced.FirstTimeSetupCompleted = true;
        await _settingsService.SaveSettings(AppSettings, CancellationToken.None);
        _settingsService.ApplySettings(AppSettings);
        
        _navigationService.ShowWindow<ChatView>();
        _navigationService.CloseWindow<SetupView>();
    }

    partial void OnSelectedAIProviderTemplateChanged(AIProvider? value)
    {
        if (value == null) return;
        
        var apiKey = AppSettings.General.AIProvider.ApiKey;
        
        AppSettings.General.AIProvider = new AIProvider
        {
            ProviderType = value.ProviderType,
            BaseUrl = value.BaseUrl,
            Model = value.Model,
            ApiKey = apiKey
        };
        
        OnPropertyChanged(nameof(AppSettings));
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