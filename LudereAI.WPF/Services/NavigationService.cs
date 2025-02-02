using System.Windows;
using LudereAI.WPF.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LudereAI.WPF.Services;

public class NavigationService(ILogger<NavigationService> logger, IServiceProvider serviceProvider) : INavigationService
{
    private readonly List<Window> _windows = new();

    public void ShowWindow<T>(bool isMainWindow = true) where T : Window
    {
        var window = serviceProvider.GetRequiredService<T>();
        
        if (_windows.Contains(window))
        {
            window.Activate();
            return;
        }
        
        
        window.Closed += OnWindowClosed;
        _windows.Add(window);

        if (isMainWindow)
        {
            Application.Current.MainWindow = window;
        }

        window.Show();
        logger.LogInformation("Window {window} opened", window.GetType().Name);
    }

    public void CloseWindow<T>() where T : Window
    {
        var window = _windows.FirstOrDefault(w => w.GetType() == typeof(T));

        window?.Close();
    }

    public void CloseWindow(Window window)
    {
        if (_windows.Contains(window))
        {
            window.Close();
        }
    }

    public void CloseAllWindows()
    {
        var windows = _windows.ToList();
        foreach (var window in windows)
        {
            window.Close();
        }
    }

    public void CloseAllWindowsButMain()
    {
        var windows = _windows.Where(window => window != Application.Current.MainWindow).ToList();
        foreach (var window in windows)
        {
            window.Close();
        }
    }

    public void CloseAllWindowsExcept<T>() where T : Window
    {
        var windows = _windows.Where(window => window.GetType() != typeof(T)).ToList();
        foreach (var window in windows)
        {
            window.Close();
        }
    }

    private void OnWindowClosed(object? sender, EventArgs e)
    {
        if (sender is not Window window)
        {
            return;
        }
        
        logger.LogInformation("Window {window} closed", window.GetType().Name);
        window.Closed -= OnWindowClosed;
        _windows.Remove(window);
        
        if (Application.Current.MainWindow == window)
        {
            Application.Current.MainWindow = _windows.FirstOrDefault();
        }
        
        if (_windows.Count == 0)
        {
            Application.Current.Shutdown();
        }
    }
}