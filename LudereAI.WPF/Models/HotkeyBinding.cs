using System.Windows.Input;

namespace LudereAI.WPF.Models;

public class HotkeyBinding
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Key Key { get; set; }
    public ModifierKeys Modifiers { get; set; }
    public Action? Callback { get; set; } = () => { };
    public bool IsGlobal { get; set; } = false;
    public bool IsEnabled { get; set; } = true;

    public override string ToString()
    {
        var modifiers = Modifiers == ModifierKeys.None ? string.Empty : $"{Modifiers}+";
        return $"{modifiers}{Key}";
    }
}