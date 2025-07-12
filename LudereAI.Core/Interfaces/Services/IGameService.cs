using LudereAI.Shared.Models;

namespace LudereAI.Core.Interfaces.Services;

public interface IGameService : IDisposable
{
    event Action<WindowInfo> OnGameStarted;
    event Action<WindowInfo> OnGameStopped;

    Task StartScanning();
    Task StopScanning();
    IEnumerable<WindowInfo> GetWindowedProcesses();
    Task<WindowInfo?> GetGameWindow();

}