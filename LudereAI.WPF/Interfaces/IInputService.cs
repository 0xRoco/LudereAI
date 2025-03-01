using System.Windows;
using LudereAI.WPF.Models;

namespace LudereAI.WPF.Interfaces;

public interface IInputService
{
    event Action<HotkeyBinding> OnHotkeyPressed;
    
    void RegisterHotkey(HotkeyBinding hotkey);
    void UnregisterHotkey(HotkeyBinding hotkey);
    void UnregisterAllHotkeys();
    
    void SetHotkeyCallback(HotkeyBinding hotkey, Action callback);
    void UpdateMainWindow(Window window);
    
    List<HotkeyBinding> GetRegisteredHotkeys();
    bool IsHotkeyRegistered(HotkeyBinding hotkey);

    void Start();
    void Stop();
}