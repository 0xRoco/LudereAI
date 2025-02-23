using System.Windows;

namespace LudereAI.WPF.Interfaces;

public interface INavigationService
{
    void ShowWindow<T>(bool isMainWindow = true, bool showDialog = false) where T : Window;
    void CloseWindow<T>() where T : Window;
    void CloseWindow(Window window);
    void CloseAllWindows();
    void CloseAllWindowsButMain();
    void CloseAllWindowsExcept<T>() where T : Window;
}