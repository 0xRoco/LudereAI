using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LudereAI.Core.Interfaces.Services;
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
    private AppSettings _settings;
    
    [ObservableProperty]
    private ObservableCollection<KeyBindingItemViewModel> _keyBindings = new();
    
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
    private void Save()
    {
        Settings.KeyBind.Hotkeys = KeyBindings.Select(kb => kb.ToKeyBinding()).ToList();
        
        _settingsService.SaveSettings(Settings, CancellationToken.None);
        
        _navigationService.CloseWindow<SettingsView>();
    }

    [RelayCommand]
    private void Cancel()
    {
        _navigationService.CloseWindow<SettingsView>();
    }

    [RelayCommand]
    private async Task Load() 
    {
        Settings = await _settingsService.LoadSettings();
        LoadKeyBindings();
    }
    
    private void LoadKeyBindings()
    {
        KeyBindings.Clear();
        
        foreach (var keyBinding in Settings.KeyBind.Hotkeys)
        {
            KeyBindings.Add(new KeyBindingItemViewModel(keyBinding, _inputService));
        }
    }
}
