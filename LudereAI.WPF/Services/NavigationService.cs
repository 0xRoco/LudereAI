using System.Windows;
using LudereAI.WPF.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LudereAI.WPF.Services;

public class NavigationService(ILogger<NavigationService> logger, IServiceProvider serviceProvider) : INavigationService, IDisposable
{
    private readonly Dictionary<Window, IServiceScope> _windowScopes = new();

    public static event Action<Window>? OnWindowClosed;
    public static event Action<Window>? OnWindowOpened;

    public void ShowWindow<T>(bool isMainWindow = true, bool showDialog = false) where T : Window
    {
        var existingWindow = _windowScopes.Keys.FirstOrDefault(w => w is T);
        if (existingWindow != null)
        {
            existingWindow.Activate();
            return;
        }

        var scope = serviceProvider.CreateScope();
        var window = scope.ServiceProvider.GetRequiredService<T>();

        _windowScopes[window] = scope;

        window.Closed += WindowClosed;

        if (isMainWindow)
        {
            Application.Current.MainWindow = window;
        }

        logger.LogInformation("Window {window} opened (MainWindow: {isMainWindow}, Dialog: {isDialog})", window.GetType().Name, isMainWindow, showDialog);

        OnWindowOpened?.Invoke(window);

        if (showDialog)
            window.ShowDialog();
        else
        {
            window.Show();
            window.Activate();
        }
    }

    public void CloseWindow<T>() where T : Window
    {
        var window = _windowScopes.Keys.FirstOrDefault(w => w.GetType() == typeof(T));

        window?.Close();
    }

    public void CloseWindow(Window window)
    {
        if (_windowScopes.ContainsKey(window))
        {
            window.Close();
        }
    }

    public void CloseAllWindows()
    {
        var windows = _windowScopes.Keys.ToList();
        foreach (var window in windows)
        {
            window.Close();
        }
    }

    public void CloseAllWindowsButMain()
    {
        var windows = _windowScopes.Keys.Where(window => window != Application.Current.MainWindow).ToList();
        foreach (var window in windows)
        {
            window.Close();
        }
    }

    public void CloseAllWindowsExcept<T>() where T : Window
    {
        var windows = _windowScopes.Keys.Where(window => window.GetType() != typeof(T)).ToList();
        foreach (var window in windows)
        {
            window.Close();
        }
    }

    private void WindowClosed(object? sender, EventArgs e)
    {
        if (sender is not Window window)
        {
            return;
        }

        logger.LogInformation("Window {window} closed", window.GetType().Name);
        OnWindowClosed?.Invoke(window);
        window.Closed -= WindowClosed;

        if (_windowScopes.Remove(window, out var scope))
        {
            scope.Dispose();
        }

        if (Application.Current.MainWindow == window)
        {
            Application.Current.MainWindow = _windowScopes.Keys.FirstOrDefault();
        }

        if (_windowScopes.Count == 0)
        {
            Application.Current.Shutdown();
        }
    }
    
    public void Dispose()
    {
        foreach (var scope in _windowScopes.Values)
        {
            scope.Dispose();
        }
        _windowScopes.Clear();
        GC.SuppressFinalize(this);
    }
}