using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LudereAI.Core.Entities;
using LudereAI.Core.Interfaces.Services;
using LudereAI.Shared.Enums;
using LudereAI.Shared.Models;
using LudereAI.WPF.Interfaces;
using LudereAI.WPF.Views;

namespace LudereAI.WPF.ViewModels;

public partial class SetupViewModel : ObservableObject
{
    private readonly ISettingsService _settings;
    private readonly INavigationService _navigation;
    
    [ObservableProperty] private AppSettings _appSettings = new();

    public ObservableCollection<AIProvider> AIProviders { get; } = new()
    {
        new AIProvider(AIProviderType.OpenAI, "OpenAI"),
        new AIProvider(AIProviderType.OpenAI, "OpenAI (ChatGPT)"),
    };
    public ObservableCollection<string> TTSProviders { get; } = ["ElevenLabs", "Piper Instance", "Custom"];
    
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsCustomAIProvider))]
    private string _selectedAIProvider = "OpenAI (ChatGPT)";
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsCustomTTSProvider))]
    private string _selectedTTSProvider = "ElevenLabs";
    
    public bool IsCustomAIProvider => SelectedAIProvider == "Custom";
    public bool IsCustomTTSProvider => SelectedTTSProvider is "Piper Instance" or "Custom";

    public SetupViewModel(ISettingsService settings, INavigationService navigation)
    {
        _settings = settings;
        _navigation = navigation;
    }

    [RelayCommand]
    private async Task SaveAndContinue()
    {
        _appSettings = await _settings.LoadSettings();
        
        
        _navigation.ShowWindow<ChatView>();
        _navigation.CloseWindow<SetupView>();
    }
}