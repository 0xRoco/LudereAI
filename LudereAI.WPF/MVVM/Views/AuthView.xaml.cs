using System.Windows;
using LudereAI.WPF.MVVM.ViewModels;

namespace LudereAI.WPF.MVVM.Views;

public partial class AuthView : Window
{
    public AuthView(AuthViewModel vm)
    {
        InitializeComponent();

        DataContext = vm;
    }
}