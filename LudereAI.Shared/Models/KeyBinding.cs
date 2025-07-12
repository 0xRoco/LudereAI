using LudereAI.Shared.Enums;

namespace LudereAI.Shared.Models;

public class KeyBinding
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public LudereKey Key { get; set; }
    public LudereModifierKeys Modifiers { get; set; }
    public bool IsGlobal { get; set; }
    public bool IsEnabled { get; set; } = true;

    public override string ToString()
    {
        var modifiers = Modifiers == LudereModifierKeys.None ? string.Empty : $"{Modifiers}+";
        return $"{modifiers}{Key}";
    }
    
    public string GetKeyDisplay() => $"{(Modifiers == LudereModifierKeys.None ? "" : $"{Modifiers}+")}{Key}";
    
    public string GetKeyDisplay(bool isRecording) => isRecording ? "Press a key..." : GetKeyDisplay();
}