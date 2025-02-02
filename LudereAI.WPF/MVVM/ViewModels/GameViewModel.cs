using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using LudereAI.WPF.Interfaces;
using Microsoft.Extensions.Logging;

namespace LudereAI.WPF.MVVM.ViewModels;

public partial class GameViewModel : ObservableObject
{
    private readonly ILogger<GameViewModel> _logger;
    private readonly IGameService _gameService;

    [ObservableProperty] private string _currentGame = string.Empty;

    public GameViewModel(ILogger<GameViewModel> logger, IGameService gameService)
    {
        _logger = logger;
        _gameService = gameService;
        
        _gameService.OnGameStarted += OnGameStarted;
        _gameService.OnGameStopped += OnGameStopped;

        _ = _gameService.StartScanning();
    }
    
    private void OnGameStarted(string gameName) =>
        Application.Current.Dispatcher.Invoke<string>(() => CurrentGame = gameName);

    private void OnGameStopped(string gameName) =>
        Application.Current.Dispatcher.Invoke<string>(() => CurrentGame = string.Empty);
}