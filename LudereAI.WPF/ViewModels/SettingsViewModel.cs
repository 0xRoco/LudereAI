using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LudereAI.Core.Entities;
using LudereAI.Core.Interfaces.Services;
using LudereAI.Services;
using LudereAI.Shared.Enums;
using LudereAI.Shared.Models;
using LudereAI.WPF.Interfaces;
using LudereAI.WPF.Models;
using LudereAI.WPF.Services;
using LudereAI.WPF.Views;
using Microsoft.Extensions.Logging;
using KeyBinding = LudereAI.Shared.Models.KeyBinding;

namespace LudereAI.WPF.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ILogger<SettingsViewModel> _logger;
    private readonly ISettingsService _settingsService;
    private readonly IInputService _inputService;
    private readonly INavigationService _navigationService;
    
    [ObservableProperty]
    private AppSettings _appSettings;
    
    [ObservableProperty]
    private AIProvider _selectedAIProviderTemplate;
    
    [ObservableProperty]
    private ObservableCollection<KeyBindingItemViewModel> _keyBindings = new();
    
    public ObservableCollection<AIProvider> AIProviderTemplates { get; } =
        new(AIProviderFactory.GetAvailableProviders());
    
    public List<string> Themes { get; } = new() { "Light", "Dark" , "System" };
    public List<string> Languages { get; } = new() { "English" };
    
    public SettingsViewModel(ILogger<SettingsViewModel> logger, ISettingsService settingsService, IInputService inputService, INavigationService navigationService)
    {
        _logger = logger;
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
        LoadKeyBindings();
    }
    
    private void LoadKeyBindings()
    {
        KeyBindings.Clear();
        
        foreach (var keyBinding in AppSettings.KeyBind.Hotkeys)
        {
            KeyBindings.Add(new KeyBindingItemViewModel(keyBinding, _inputService));
        }
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
