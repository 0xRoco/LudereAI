using System.Windows;
using LudereAI.WPF.ViewModels;

namespace LudereAI.WPF.Views;

public partial class SetupView : Window
{
    private SetupViewModel _vm;
    
    public SetupView(SetupViewModel vm)
    {
        InitializeComponent();
        DataContext = _vm = vm;
    }
}