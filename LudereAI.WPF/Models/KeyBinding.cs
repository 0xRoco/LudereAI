using System.Windows;
using System.Windows.Input;

namespace LudereAI.WPF.Models;

public class KeyBinding
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Key Key { get; set; }
    public ModifierKeys Modifiers { get; set; }
    public bool IsGlobal { get; set; }
    public string? Window { get; set; }
    public bool IsEnabled { get; set; } = true;

    public override string ToString()
    {
        var modifiers = Modifiers == ModifierKeys.None ? string.Empty : $"{Modifiers}+";
        return $"{modifiers}{Key}";
    }
    
    public string GetKeyDisplay() => $"{(Modifiers == ModifierKeys.None ? "" : $"{Modifiers}+")}{Key}";
    
    public string GetKeyDisplay(bool isRecording) => isRecording ? "Press a key..." : GetKeyDisplay();

    public bool CanExecute()
    {
        var mainWindow = Application.Current?.MainWindow?.GetType().Name;
        var isTargetWindow = Window == null || Window == mainWindow;
        return IsEnabled && (IsGlobal || isTargetWindow);
    }
}