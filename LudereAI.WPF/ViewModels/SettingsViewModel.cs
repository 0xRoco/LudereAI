using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LudereAI.WPF.Interfaces;
using LudereAI.WPF.Models;
using Microsoft.Extensions.Logging;
using KeyBinding = LudereAI.WPF.Models.KeyBinding;

namespace LudereAI.WPF.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ILogger<SettingsViewModel> _logger;
    private readonly ISettingsService _settingsService;
    private readonly IInputService _inputService;
    
    [ObservableProperty]
    private AppSettings _settings;
    [ObservableProperty]
    private ObservableCollection<KeyBindingItemViewModel> _keyBindings = new();
    
    public List<string> Themes { get; } = new() { "Light", "Dark" , "System" };
    public List<string> Languages { get; } = new() { "English" };
    
    [RelayCommand]
    private void Save() => _settingsService.SaveSettings(Settings);
    
    [RelayCommand]
    private void Load() => Settings = _settingsService.LoadSettings();
    
    public SettingsViewModel(ILogger<SettingsViewModel> logger, ISettingsService settingsService, IInputService inputService)
    {
        _logger = logger;
        _settingsService = settingsService;
        _inputService = inputService;

        _settings = _settingsService.LoadSettings();

        _settings.KeyBind.Hotkeys = new()
        {
            new KeyBinding
            {
                Id = "ToggleOverlay", Name = "Toggle Overlay", Key = Key.O, Modifiers = ModifierKeys.Alt,
                IsGlobal = true, IsEnabled = true
            },

            new KeyBinding
            {
                Id = "NewChat", Name = "New Chat", Key = Key.N, Modifiers = ModifierKeys.Control, IsGlobal = true,
                IsEnabled = true
            }
        };
        
        foreach (var keyBinding in _settings.KeyBind.Hotkeys)
        {
            KeyBindings.Add(new KeyBindingItemViewModel(keyBinding, inputService));
        }
    }
}