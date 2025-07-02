using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using LudereAI.Core.Interfaces.Services;
using LudereAI.Shared.DTOs;
using LudereAI.Shared.Models;
using Microsoft.Extensions.Logging;

namespace LudereAI.Services.Services;

public class GameService : IGameService
{
    private readonly ILogger<IGameService> _logger;
    private readonly ISettingsService _settingsService;
    private readonly IOpenAIService _openAIService;
    private CancellationTokenSource _cts = new();
    private bool _isScanning;
    private bool _isGameRunning;
    private WindowInfo? _currentGame;
    private int _gameScanInterval = 5;
    private DateTime _lastGameCheck = DateTime.MinValue;
    private const int MinimumCheckInterval = 5;

    
    public event Action<WindowInfo>? OnGameStarted;
    public event Action<WindowInfo>? OnGameStopped;

    public GameService(
        ILogger<IGameService> logger,
        ISettingsService settingsService, IOpenAIService openAIService)
    {
        _logger = logger;
        _settingsService = settingsService;
        _openAIService = openAIService;


        _settingsService.OnSettingsApplied += async settings =>
        {
            if (settings.GameIntegration.Enabled)
            { 
                _gameScanInterval = settings.GameIntegration.ScanInterval;
                await StartScanning(); 
                return;
            }
            
            await StopScanning();
        };
    }
    
    public async Task StartScanning()
    {
        if (_isScanning) return;
        _isScanning = true;
        
        _logger.LogInformation("Starting game detection service");
        
        _cts = new CancellationTokenSource();

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_gameScanInterval));
        while (await timer.WaitForNextTickAsync(_cts.Token))
        { 
            if (_gameScanInterval <= 0)
            {
                _logger.LogWarning("Game scan interval is set to 0 or less. Disabling game detection service");
                await StopScanning();
                return;
            }

            await CheckGameStatus();
        }
    }

    public async Task StopScanning()
    {
        if (!_isScanning) return;
        
        if (_currentGame != null)
        {
            OnGameStopped?.Invoke(_currentGame);
        }
        
        await _cts.CancelAsync();
        _isScanning = false;
        _isGameRunning = false;
        _currentGame = null;
        
        _logger.LogInformation("Stopped game scanning");
    }
    
    public IEnumerable<WindowInfo> GetWindowedProcesses()
    {
        var windows = new List<WindowInfo>();
        EnumWindows(((wnd, param) =>
        {
            if (!IsWindowVisible(wnd)) return true;
            
            var sb = new StringBuilder(256);
            if (GetWindowText(wnd, sb, 256) > 0)
            {
                var title = sb.ToString();
                if (string.IsNullOrWhiteSpace(title)) return true;
                
                GetWindowThreadProcessId(wnd, out var processId);
                try
                {
                    using var process = Process.GetProcessById((int)processId);
                    windows.Add(new WindowInfo
                    {
                        Handle = wnd,
                        Title = title,
                        ProcessName = process.ProcessName,
                        ProcessId = process.Id
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to get process by id {ProcessId}", processId);
                }
            }
            return true;
        }), IntPtr.Zero);
        return windows;
    }
    
    public async Task<WindowInfo?> GetGameWindow()
    {
        try
        {
            var windows = GetWindowedProcesses().ToList();
            var processes = windows.Select(w => new ProcessInfoDTO
            {
                ProcessId = w.ProcessId,
                ProcessName = w.ProcessName,
                Title = w.Title
            }).ToList();
            
            var predictedProcess = await _openAIService.PredictGame(processes);
            
            var process = windows.FirstOrDefault(w => w.ProcessId == predictedProcess.ProcessId);
            if (process == null) return null;
            
            process.Title = predictedProcess.Title;
            return process;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to get current game: {Message}", ex.Message);
            return null;
        }
    }
    
    private async Task CheckGameStatus()
    {
        if (_isGameRunning && (DateTime.Now - _lastGameCheck).TotalSeconds < MinimumCheckInterval)
        {
            return;
        }

        _lastGameCheck = DateTime.Now;

        if (_currentGame != null)
        {
            var windows = GetWindowedProcesses();
            var currentGameStillRunning = windows.Any(w => 
                w.ProcessId == _currentGame.ProcessId && 
                w.Title == _currentGame.Title);

            if (currentGameStillRunning) return;
            
            _isGameRunning = false;
            var gameToNotify = _currentGame;
            _currentGame = null;
            OnGameStopped?.Invoke(gameToNotify);
            return;

        }

        var gameWindow = await GetGameWindow();
        if (gameWindow != null && !_isGameRunning)
        {
            _isGameRunning = true;
            _currentGame = gameWindow;
            OnGameStarted?.Invoke(gameWindow);
        }
    }

    public void Dispose()
    {
        _cts.Dispose();
        GC.SuppressFinalize(this);
    }
    
    //TODO: Use LibraryImport instead of DllImport attribute for better performance + compatibility
    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
    
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
}