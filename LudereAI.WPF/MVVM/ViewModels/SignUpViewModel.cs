using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LudereAI.Shared.DTOs;
using LudereAI.WPF.Interfaces;
using Microsoft.Extensions.Logging;

namespace LudereAI.WPF.MVVM.ViewModels;

public partial class SignUpViewModel : ObservableObject
{
    private readonly ILogger<SignUpViewModel> _logger;
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;
    private readonly AuthViewModel _authViewModel;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(CanSignUp))]
    private string _firstName;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(CanSignUp))]
    private string _lastName;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(CanSignUp))]
    private string _username;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(CanSignUp))]
    private string _email;
    [ObservableProperty, NotifyPropertyChangedFor(nameof(CanSignUp))]
    private string _password;

    public SignUpViewModel(ILogger<SignUpViewModel> logger, IAuthService authService, INavigationService navigationService, AuthViewModel authViewModel)
    {
        _logger = logger;
        _authService = authService;
        _navigationService = navigationService;
        _authViewModel = authViewModel;
    }
    
    public bool CanSignUp => !string.IsNullOrWhiteSpace(FirstName) 
                             && !string.IsNullOrWhiteSpace(LastName) 
                             && !string.IsNullOrWhiteSpace(Username) 
                             && !string.IsNullOrWhiteSpace(Email) 
                             && !string.IsNullOrWhiteSpace(Password);
    
    [RelayCommand]
    private async Task SignUp()
    {
        var success = await _authService.SignUpAsync(new SignUpDTO()
        {
            FirstName = FirstName,
            LastName = LastName,
            Username = Username,
            Email = Email,
            Password = Password
        });
        
        if (!success) return;
        
        _authViewModel.ShowLoginView();
    }
    
    [RelayCommand]
    private void ShowLoginView()
    {
        _authViewModel.ShowLoginView();
    }
    
    [RelayCommand]
    private void ShowOnBoardingView()
    {
        _authViewModel.ShowOnBoardingView();
    }
    
}