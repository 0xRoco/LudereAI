using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LudereAI.WPF.Interfaces;
using Microsoft.Extensions.Logging;

namespace LudereAI.WPF.ViewModels;

public partial class AuthViewModel : ObservableObject
{
    private readonly ILogger<AuthViewModel> _logger;
    private readonly ITokenService _tokenService;
    private readonly INavigationService _navigationService;
    
    [ObservableProperty]
    private int _currentView;
    
    public AuthViewModel(ILogger<AuthViewModel> logger, IAuthService authService, ISessionService sessionService, INavigationService navigationService, ITokenService tokenService)
    {
        _navigationService = navigationService;
        _tokenService = tokenService;
        _logger = logger;

        _ = TryAutoLogin();
    }
    
    private async Task TryAutoLogin()
    {
        if (!await _tokenService.ValidateToken())
        {
            ShowOnBoardingView();
        }
    }
    
    [RelayCommand]
    public void ShowOnBoardingView()
    {
        CurrentView = 0;
    }
    
    [RelayCommand]
    public void ShowLoginView()
    {
        CurrentView = 1;
    }
    
        
    [RelayCommand]
    public void ShowSignUpView()
    {
        CurrentView = 2;
    }
    
}