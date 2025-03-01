using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using LudereAI.WPF.Interfaces;
using LudereAI.WPF.Models;
using Microsoft.Extensions.Logging;
using static System.GC;

namespace LudereAI.WPF.Services;

public class InputService : IInputService, IDisposable
{
    private readonly ILogger<IInputService> _logger;
    private readonly ConcurrentDictionary<string, HotkeyBinding> _bindings = new();
    private HwndSource? _hwndSource;
    private bool _isListening;

    public InputService(ILogger<IInputService> logger)
    {
        _logger = logger;

        NavigationService.OnWindowOpened += WindowOpened;
    }

    private void WindowOpened(Window window)
    {
        if (window == Application.Current.MainWindow && _isListening)
        {
            _logger.LogInformation("Main window changed, updating input handler");
            UpdateMainWindow(window);
        }    
    }

    public void Start()
    {
        if (_isListening) return;
        
        _logger.LogInformation("Starting input service");
        
        UpdateMainWindow(Application.Current.MainWindow);
        
        foreach (var binding in _bindings.Values)
        {
            RegisterSystemHotkey(binding);
        }
        
        _isListening = true;
    }

    public void Stop()
    {
        throw new NotImplementedException();
    }

    public event Action<HotkeyBinding>? OnHotkeyPressed;
    public void RegisterHotkey(HotkeyBinding hotkey)
    {
        var hotkeyId = hotkey.Id;

        _bindings.TryAdd(hotkeyId, hotkey);
    }

    public void UnregisterHotkey(HotkeyBinding hotkey)
    {
        var hotkeyId = hotkey.Id;
        var hotkeyHash = hotkey.GetHashCode();
        
        if (_bindings.TryRemove(hotkeyId, out _))
        {
            UnregisterHotKey(IntPtr.Zero, hotkeyHash);
            _logger.LogInformation("Unregistered hotkey: {Hotkey}", hotkey);
        }
        else
        {
            _logger.LogWarning("Hotkey not found: {Hotkey}", hotkey);
        }
    }

    public void UnregisterAllHotkeys()
    {
        foreach (var binding in _bindings.Values)
        {
            UnregisterHotkey(binding);
        }
        
        _bindings.Clear();
    }

    public void SetHotkeyCallback(HotkeyBinding hotkey, Action callback)
    {
        var hotkeyId = hotkey.Id;
        
        if (_bindings.TryGetValue(hotkeyId, out var binding))
        {
            binding.Callback = callback;
            _logger.LogInformation("Set callback for hotkey: {Hotkey}", binding);
        }
        else
        {
            _logger.LogWarning("Hotkey not found: {Hotkey}", hotkey);
        }
    }

    public void UpdateMainWindow(Window window)
    {
        if (window == null)
            throw new InvalidOperationException("Main window not found");

        _logger.LogInformation("Updating main window to {WindowType}", window.GetType().Name);
    
        // Remove hook from previous source
        if (_hwndSource != null)
        {
            _hwndSource.RemoveHook(WndProc);
        
            // Unregister hotkeys from old window if we're listening
            if (_isListening)
            {
                foreach (var binding in _bindings.Values.Where(b => b.IsGlobal))
                {
                    UnregisterHotKey(_hwndSource.Handle, binding.GetHashCode());
                }
            }
        }
    
        // Create new source
        var helper = new WindowInteropHelper(window);
    
        // Window might not be fully initialized yet
        if (helper.Handle == IntPtr.Zero)
        {
            // For windows that aren't fully loaded yet
            window.SourceInitialized += (sender, _) =>
            {
                SetupHWNDSource((Window)sender!);
            };
        
            return;
        }
    
        SetupHWNDSource(window);
    }

    public List<HotkeyBinding> GetRegisteredHotkeys() => _bindings.Values.ToList();

    public bool IsHotkeyRegistered(HotkeyBinding hotkey)
    {
        var hotkeyId = hotkey.Id;
        return _bindings.ContainsKey(hotkeyId);
    }
    
    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int wmHotkey = 0x0312;

        if (msg != wmHotkey) return IntPtr.Zero;
        var hotkeyId = wParam.ToInt32();


        if (!TryGetHotkeyBinding(hotkeyId, out var binding)) return IntPtr.Zero;
        
        if (binding is not { IsEnabled: true }) return IntPtr.Zero;
                
        _logger.LogDebug("Hotkey pressed: {Hotkey}", binding);
        binding.Callback?.Invoke();
        OnHotkeyPressed?.Invoke(binding);
        handled = true;

        return IntPtr.Zero;
    }

    private bool TryGetHotkeyBinding(int hotkeyHash, out HotkeyBinding? binding)
    {
        binding = _bindings.Values.FirstOrDefault(b => b.GetHashCode() == hotkeyHash);
        return binding != null;
    }
    
    private void RegisterSystemHotkey(HotkeyBinding binding)
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
    
    private void SetupHWNDSource(Window window)
    {
        var helper = new WindowInteropHelper(window);
        _hwndSource = HwndSource.FromHwnd(helper.Handle);

        if (_hwndSource == null)
            throw new InvalidOperationException("Failed to create HwndSource");

        _hwndSource.AddHook(WndProc);

        // Re-register hotkeys if we're already listening
        if (_isListening)
        {
            foreach (var binding in _bindings.Values.Where(b => b.IsGlobal))
            {
                RegisterSystemHotkey(binding);
            }
        }
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