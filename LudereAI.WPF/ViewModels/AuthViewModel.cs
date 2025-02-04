using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LudereAI.WPF.Interfaces;
using LudereAI.WPF.Views;
using Microsoft.Extensions.Logging;

namespace LudereAI.WPF.ViewModels;

public partial class AuthViewModel : ObservableObject
{
    private readonly ILogger<AuthViewModel> _logger;
    private readonly ITokenService _tokenService;
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;
    
    [ObservableProperty]
    private int _currentView;
    
    public AuthViewModel(ILogger<AuthViewModel> logger, IAuthService authService, INavigationService navigationService, ITokenService tokenService)
    {
        _navigationService = navigationService;
        _tokenService = tokenService;
        _logger = logger;
        _authService = authService;

        _ = TryAutoLogin();
    }
    
    private async Task TryAutoLogin()
    {
        var account = await _tokenService.ValidateToken();
        if (account != null)
        {
            _navigationService.ShowWindow<ChatView>();
            _navigationService.CloseWindow<AuthView>();
        }
        else
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

    [RelayCommand]
    public async Task GuestLogin()
    {
        if (await _authService.GuestLoginAsync())
        {
            await TryAutoLogin();
        }
    }
    
}