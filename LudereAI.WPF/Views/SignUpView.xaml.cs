using System.Windows;
using System.Windows.Controls;
using LudereAI.WPF.ViewModels;

namespace LudereAI.WPF.Views;

public partial class SignUpView : UserControl
{
    public SignUpView(SignUpViewModel vm)
    {
        InitializeComponent();
        
        DataContext = vm;
    }

    private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is not SignUpViewModel vm)
        {
            return;
        }
        
        if (sender is PasswordBox passwordBox)
        {
            vm.Password = passwordBox.Password;
        }
    }
}