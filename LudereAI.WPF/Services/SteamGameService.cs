using LudereAI.WPF.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace LudereAI.WPF.Services;

public class SteamGameService : IGameService
{
    private readonly ILogger<IGameService> _logger;
    private CancellationTokenSource _cts = new();
    private readonly List<string> _libraryFolders = new();
    private bool _isScanning;
    private bool _isGameRunning;
    private string? _currentGame;
    private const int GameScanInterval = 500;
    
    public event Action<string>? OnGameStarted;
    public event Action<string>? OnGameStopped;

    public SteamGameService(ILogger<IGameService> logger)
    {
        _logger = logger;
    }
    
    public async Task StartScanning()
    {
        if (_isScanning) return;
        _isScanning = true;
        
        _logger.LogInformation("Starting Steam game scanning");
        
        _cts = new CancellationTokenSource();

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(GameScanInterval));
        while (await timer.WaitForNextTickAsync(_cts.Token))
        { 
            CheckGameStatus();
        }
    }

    public async Task StopScanning()
    {
        await _cts.CancelAsync();
        OnGameStopped?.Invoke("");
        _isScanning = false;
        _isGameRunning = false;
        _currentGame = null;
        
        _logger.LogInformation("Stopped Steam game scanning");
    }

    
    private void CheckGameStatus()
    {
        var gameName = TryGetCurrentGame();
        var gameDetected = !string.IsNullOrWhiteSpace(gameName);
        var shouldNotify = false;
        var gameStopped = false;

        switch (gameDetected)
        {
            case true when !_isGameRunning:
                _isGameRunning = true;
                shouldNotify = true;
                break;
            case false when _isGameRunning:
                _isGameRunning = false;
                gameStopped = true;
                break;
        }

        if (shouldNotify)
        {
            _currentGame = gameName;
            OnGameStarted?.Invoke(gameName);
        }else if (gameStopped)
        {
            if (_currentGame != null) OnGameStopped?.Invoke(_currentGame);
            _currentGame = null;
        }
    }
    
    private string TryGetCurrentGame()
    {
        var gameName = "";
        
        try
        {
            var steamPath = GetSteamRegistryValue("SteamPath");
            var runningAppId = GetSteamRegistryValue("RunningAppID");

            if (string.IsNullOrWhiteSpace(steamPath) || !int.TryParse(runningAppId, out var appId))
            {
                return gameName;
            }
            

            gameName = GetGameNameFromRegistry(runningAppId) ?? "";
            return gameName;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to get current Steam game: {Message}", ex.Message);
            return gameName;
        }
    }
    
    private static string? GetSteamRegistryValue(string valueName)
    {
        return Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", valueName, "")?.ToString();
    }
    
    private static string? GetGameNameFromRegistry(string appId)
    {
        return Registry.GetValue($@"HKEY_CURRENT_USER\Software\Valve\Steam\Apps\{appId}", "Name", "")?.ToString();
    }
    

    public void Dispose()
    {
        _cts.Dispose();
        
        GC.SuppressFinalize(this);
    }
}