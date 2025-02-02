namespace LudereAI.WPF.Interfaces;

public interface IGameService : IDisposable
{
    event Action<string> OnGameStarted;
    event Action<string> OnGameStopped;

    Task StartScanning();
    Task StopScanning();
}