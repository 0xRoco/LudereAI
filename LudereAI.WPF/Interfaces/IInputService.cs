﻿using System.Windows;
using LudereAI.WPF.Models;

namespace LudereAI.WPF.Interfaces;

public interface IInputService
{
    event Action<KeyBinding> OnHotkeyPressed;
    
    void RegisterHotkey(KeyBinding key);
    void UnregisterHotkey(KeyBinding key);
    void UnregisterAllHotkeys();
    
    List<KeyBinding> GetRegisteredHotkeys();
    bool IsHotkeyRegistered(KeyBinding key);

    void Start();
    void Stop();
}