using System.Windows.Input;
using LudereAI.Shared.Models;

namespace LudereAI.Core.Interfaces.Services;

public interface IInputService
{
    event Action<KeyBinding> OnHotkeyPressed;
    
    void ApplySettings(KeyBindSettings settings);
    void RegisterHotkey(KeyBinding key);
    void UnregisterHotkey(KeyBinding key);
    void UnregisterAllHotkeys();
    
    List<KeyBinding> GetRegisteredHotkeys();
    bool IsHotkeyRegistered(KeyBinding key);

    void Start();
    void Stop();
}