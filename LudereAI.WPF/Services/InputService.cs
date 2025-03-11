using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using LudereAI.WPF.Interfaces;
using Microsoft.Extensions.Logging;
using static System.GC;
using KeyBinding = LudereAI.WPF.Models.KeyBinding;

namespace LudereAI.WPF.Services;

public class InputService : IInputService, IDisposable
{
    private readonly ILogger<IInputService> _logger;
    private readonly ConcurrentDictionary<string, KeyBinding> _bindings = new();
    private HwndSource? _hwndSource;
    private bool _isListening;
    private Window? _messageWindow;
    
    public InputService(ILogger<IInputService> logger)
    {
        _logger = logger;
    }
    

    public void Start()
    {
        if (_isListening) return;
        
        _logger.LogInformation("Starting input service");
        
        CreateMessageWindow();
        _isListening = true;
        
        InitializeDefaultKeyBinds();
    }

    public void Stop()
    {
        if (!_isListening) return;
        
        UnregisterAllHotkeys();
        
        if (_hwndSource != null)
        {
            _hwndSource.RemoveHook(WndProc);
            _hwndSource = null;
        }
        
        if (_messageWindow != null)
        {
            _messageWindow.Close();
            _messageWindow = null;
        }
        
        _isListening = false;
        _logger.LogInformation("Stopped input service");
    }

    public event Action<KeyBinding>? OnHotkeyPressed;
    public event Action<ModifierKeys, Key>? OnKeyPressed;

    public void RegisterHotkey(KeyBinding key)
    {
        var hotkeyId = key.Id;
        
        if (IsHotkeyRegistered(key) || !_isListening) return;

        _bindings.TryAdd(hotkeyId, key);
        { 
            RegisterSystemHotkey(key);
        }
    }

    public void UnregisterHotkey(KeyBinding key)
    {
        var hotkeyId = key.Id;
        var hotkeyHash = key.GetHashCode();
        
        if (_bindings.TryRemove(hotkeyId, out _))
        {
            UnregisterHotKey(IntPtr.Zero, hotkeyHash);
            _logger.LogInformation("Unregistered key: {Hotkey}", key);
        }
        else
        {
            _logger.LogWarning("Hotkey not found: {Hotkey}", key);
        }
    }

    public void UnregisterAllHotkeys()
    {
        foreach (var binding in _bindings.Values.ToList())
        {
            UnregisterHotkey(binding);
        }
        
        _bindings.Clear();
    }

    public void SetHotkeyCallback(string keyId, Action callback)
    {
        
        if (_bindings.TryGetValue(keyId, out var binding))
        {
            //binding.Callback = callback;
            _logger.LogInformation("Set callback for key: {Hotkey}", binding);
        }
        else
        {
            _logger.LogWarning("Hotkey not found: {Hotkey}", keyId);
        }
    }
    

    public List<KeyBinding> GetRegisteredHotkeys() => _bindings.Values.ToList();

    public bool IsHotkeyRegistered(KeyBinding key)
    {
        var hotkeyId = key.Id;
        return _bindings.ContainsKey(hotkeyId);
    }

    private void InitializeDefaultKeyBinds()
    {
        var defaultKeyBinds = new List<KeyBinding>
        {
            new KeyBinding
            {
                Id = "ToggleOverlay",
                Name = "Toggle Overlay",
                Key = Key.O,
                Modifiers = ModifierKeys.Alt,
                IsGlobal = false,
                Window = "ChatView",
                IsEnabled = true
            },
            new KeyBinding
            {
                Id = "NewChat",
                Name = "New Chat",
                Key = Key.N,
                Modifiers = ModifierKeys.Control,
                IsGlobal = false,
                Window = "ChatView",
                IsEnabled = true
            }
        };
        
        foreach (var keyBinding in defaultKeyBinds)
        {
            RegisterHotkey(keyBinding);
        }
        
        _logger.LogInformation("Initialized default key binds");
    }
    
    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int wmHotkey = 0x0312;

        if (msg != wmHotkey) return IntPtr.Zero;
        var hotkeyId = wParam.ToInt32();
        

        if (!TryGetHotkeyBinding(hotkeyId, out var binding)) return IntPtr.Zero;
        
        if (binding == null || !binding.CanExecute()) return IntPtr.Zero;
                
        _logger.LogDebug("Hotkey pressed: ({hotKeyName}) {Hotkey}", binding.Name, binding);
        //binding.Callback?.Invoke();
        OnHotkeyPressed?.Invoke(binding);
        handled = true;

        return IntPtr.Zero;
    }

    private bool TryGetHotkeyBinding(int hotkeyHash, out KeyBinding? binding)
    {
        binding = _bindings.Values.FirstOrDefault(b => b.GetHashCode() == hotkeyHash);
        return binding != null;
    }
    
    private void RegisterSystemHotkey(KeyBinding binding)
    {
        try
        {
            if (!_isListening || _hwndSource?.Handle == IntPtr.Zero)
            {
                _logger.LogWarning("Cannot register hotkey - input service not properly initialized");
                return;
            }

            var hotkeyHash = binding.GetHashCode();
            var modifiers = (int)binding.Modifiers;
            var key = KeyInterop.VirtualKeyFromKey(binding.Key);

            if (_hwndSource != null && RegisterHotKey(_hwndSource.Handle, hotkeyHash, modifiers, key))
            {
                _logger.LogInformation("Registered hotkey: {Hotkey}", binding);
            }
            else
            {
                _logger.LogWarning("Failed to register hotkey: {Hotkey}", binding);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering hotkey: {Hotkey}", binding);
        }
    }
    
    private void CreateMessageWindow()
    {
        // Create a hidden window to receive hotkey messages
        _messageWindow = new Window
        {
            Width = 0,
            Height = 0,
            WindowStyle = WindowStyle.None,
            ShowInTaskbar = false,
            ShowActivated = false,
            Visibility = Visibility.Hidden
        };

        // Show the window (but it's invisible)
        _messageWindow.Show();
        
        // Get the window handle and create an HwndSource
        var helper = new WindowInteropHelper(_messageWindow);
        _hwndSource = HwndSource.FromHwnd(helper.Handle);

        if (_hwndSource == null)
            throw new InvalidOperationException("Failed to create HwndSource");

        _hwndSource.AddHook(WndProc);
        
        _logger.LogInformation("Created message window for hotkey handling");
    }
    
    public void Dispose()
    {
        Stop();
        SuppressFinalize(this);
    }
    
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
    
    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}