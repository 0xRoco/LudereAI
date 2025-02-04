using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LudereAI.Shared.DTOs;
using LudereAI.WPF.Interfaces;
using LudereAI.WPF.Views;
using Microsoft.Extensions.Logging;

namespace LudereAI.WPF.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly ILogger<LoginViewModel> _logger;
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;
    private readonly AuthViewModel _authViewModel;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanLogin))]
    private string _username;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanLogin))]
    private string _password;

    [ObservableProperty] private bool _rememberMe;
    

    public LoginViewModel(ILogger<LoginViewModel> logger, IAuthService authService, INavigationService navigationService, AuthViewModel authViewModel)
    {
        _logger = logger;
        _authService = authService;
        _navigationService = navigationService;
        _authViewModel = authViewModel;
    }

    public bool CanLogin =>
        !string.IsNullOrWhiteSpace(Username)
        && !string.IsNullOrWhiteSpace(Password);
    
    
    [RelayCommand]
    private async Task Login()
    {
        var success = await _authService.LoginAsync(new LoginDTO {
            Username = Username,
            Password = Password,
        });
        if (!success) return;

        _navigationService.ShowWindow<ChatView>(); 
        _navigationService.CloseWindow<AuthView>();
    }
    
    [RelayCommand]
    private void ShowSignUpView()
    {
        _authViewModel.ShowSignUpView();
    }

}