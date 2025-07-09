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
    private AIProvider _selectedAIProviderTemplate;
    
    public ObservableCollection<AIProvider> AIProviderTemplates { get; } = new(AIProviderFactory.GetAvailableProviders());
    public ObservableCollection<string> TTSProviders { get; } = ["ElevenLabs", "Piper Instance", "Custom"];
    
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsCustomTTSProvider))]
    private string _selectedTTSProvider = "ElevenLabs";
    public bool IsCustomTTSProvider => SelectedTTSProvider is "Piper Instance" or "Custom";

    public SetupViewModel(ISettingsService settingsService, INavigationService navigationService)
    {
        _settingsService = settingsService;
        _navigationService = navigationService;

        AppSettings.General.AIProvider ??= new AIProvider();
        
        SelectedAIProviderTemplate = AIProviderTemplates.First(p => p.ProviderType == AIProviderType.OpenAI);
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
        if (value == null || AppSettings.General.AIProvider == null) return;
        
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
}