using LudereAI.WPF.Models;

namespace LudereAI.WPF.Interfaces;

public interface IGameService : IDisposable
{
    event Action<WindowInfo> OnGameStarted;
    event Action<WindowInfo> OnGameStopped;

    Task StartScanning();
    Task StopScanning();
    IEnumerable<WindowInfo> GetWindowedProcessesAsync();

}