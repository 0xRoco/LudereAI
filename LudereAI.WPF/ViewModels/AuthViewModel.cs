using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LudereAI.WPF.Interfaces;
using LudereAI.WPF.Views;
using Microsoft.Extensions.Logging;

namespace LudereAI.WPF.ViewModels;

public partial class AuthViewModel : ObservableObject
{
    private readonly ILogger<AuthViewModel> _logger;
    private readonly INavigationService _navigationService;
    
    [ObservableProperty]
    private int _currentView;
    
    public AuthViewModel(ILogger<AuthViewModel> logger, INavigationService navigationService)
    {
        _navigationService = navigationService;
        _logger = logger;
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