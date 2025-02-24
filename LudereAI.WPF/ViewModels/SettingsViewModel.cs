using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LudereAI.WPF.Interfaces;
using LudereAI.WPF.Models;
using Microsoft.Extensions.Logging;

namespace LudereAI.WPF.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ILogger<SettingsViewModel> _logger;
    private readonly ISettingsService _settingsService;
    
    [ObservableProperty]
    private AppSettings _settings;
    
    public List<string> Themes { get; } = new() { "Light", "Dark" , "System" };
    public List<string> Languages { get; } = new() { "English" };
    
    [RelayCommand]
    private void Save() => _settingsService.SaveSettings(Settings);
    
    [RelayCommand]
    private void Load() => Settings = _settingsService.LoadSettings();
    
    public SettingsViewModel(ILogger<SettingsViewModel> logger, ISettingsService settingsService)
    {
        _logger = logger;
        _settingsService = settingsService;
        
        _settings = _settingsService.LoadSettings();
    }
}