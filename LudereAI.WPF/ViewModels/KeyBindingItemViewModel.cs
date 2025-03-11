using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LudereAI.WPF.Interfaces;
using KeyBinding = LudereAI.WPF.Models.KeyBinding;

namespace LudereAI.WPF.ViewModels;

public partial class KeyBindingItemViewModel : ObservableObject
{
    private readonly IInputService _inputService;
    
    [ObservableProperty]
    private string _id = string.Empty;
    [ObservableProperty]
    private string _name = string.Empty;
    [ObservableProperty] 
    private Key _key;
    [ObservableProperty] 
    private ModifierKeys _modifiers;
    [ObservableProperty]
    private bool _isGlobal;
    [ObservableProperty]
    private bool _isEnabled;
    [ObservableProperty] 
    private bool _isRecording;
    
    public string KeyDisplay => IsRecording ? "Press a key..." : $"{(Modifiers == ModifierKeys.None ? "" : $"{Modifiers}+")}{Key}";

    public KeyBindingItemViewModel(KeyBinding keyBinding, IInputService inputService)
    {
        _inputService = inputService;
        Id = keyBinding.Id;
        Name = keyBinding.Name;
        Key = keyBinding.Key;
        Modifiers = keyBinding.Modifiers;
        IsGlobal = keyBinding.IsGlobal;
        IsEnabled = keyBinding.IsEnabled;
    }
    
    public KeyBinding ToKeyBinding()
    {
        return new KeyBinding
        {
            Id = Id,
            Name = Name,
            Key = Key,
            Modifiers = Modifiers,
            IsGlobal = IsGlobal,
            IsEnabled = IsEnabled
        };
    }

    [RelayCommand]
    private void StartRecording()
    {
        IsRecording = !IsRecording;
    }
}