using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LudereAI.WPF.Interfaces;
using LudereAI.WPF.Models;
using LudereAI.WPF.Views;
using Microsoft.Extensions.Logging;
using KeyBinding = LudereAI.WPF.Models.KeyBinding;

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

    [RelayCommand]
    private void Save()
    {
        Settings.KeyBind.Hotkeys = KeyBindings.Select(kb => kb.ToKeyBinding()).ToList();
        
        _settingsService.SaveSettings(Settings);
        
        _navigationService.CloseWindow<SettingsView>();
    }

    [RelayCommand]
    private void Cancel()
    {
        _navigationService.CloseWindow<SettingsView>();
    }

    [RelayCommand]
    private void Load() 
    {
        Settings = _settingsService.LoadSettings();
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
    
    
    public SettingsViewModel(ILogger<SettingsViewModel> logger, ISettingsService settingsService, IInputService inputService, INavigationService navigationService)
    {
        _logger = logger;
        _settingsService = settingsService;
        _inputService = inputService;
        _navigationService = navigationService;

        Settings = _settingsService.LoadSettings();
        
        // Ensure defaults if no hotkeys exist
        if (Settings.KeyBind.Hotkeys.Count == 0)
        {
            Settings.KeyBind.Hotkeys = new List<KeyBinding>
            {
                new()
                {
                    Id = "ToggleOverlay", 
                    Name = "Toggle Overlay", 
                    Key = Key.O, 
                    Modifiers = ModifierKeys.Alt,
                    IsGlobal = true, 
                    IsEnabled = true
                },
                new()
                {
                    Id = "NewChat", 
                    Name = "New Chat", 
                    Key = Key.N, 
                    Modifiers = ModifierKeys.Control, 
                    IsGlobal = true,
                    IsEnabled = true
                }
            };
        }
        
        LoadKeyBindings();
    }
}
