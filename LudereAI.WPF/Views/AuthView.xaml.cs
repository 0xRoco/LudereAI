using System.Windows;
using LudereAI.WPF.ViewModels;

namespace LudereAI.WPF.Views;

public partial class AuthView : Window
{
    public AuthView(AuthViewModel vm)
    {
        InitializeComponent();

        DataContext = vm;
    }
}