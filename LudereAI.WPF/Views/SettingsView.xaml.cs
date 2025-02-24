using System.Windows;
using LudereAI.WPF.ViewModels;

namespace LudereAI.WPF.Views;

public partial class SettingsView : Window
{
    public SettingsView(SettingsViewModel vm)
    {
        InitializeComponent();

        DataContext = vm;
    }
}