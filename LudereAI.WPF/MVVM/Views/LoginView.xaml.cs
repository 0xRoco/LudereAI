using System.Windows;
using System.Windows.Controls;
using LudereAI.WPF.MVVM.ViewModels;

namespace LudereAI.WPF.MVVM.Views;

public partial class LoginView : UserControl
{
    
    public LoginView(LoginViewModel vm)
    {
        InitializeComponent();
        
        DataContext = vm;
    }

    private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is not LoginViewModel vm)
        {
            return;
        }

        if (sender is PasswordBox passwordBox)
        {
            vm.Password = passwordBox.Password;
        }
    }
}