using System.Windows;
using System.Windows.Controls;

namespace LudereAI.WPF.Infrastructure;

public class ViewLocator : ContentControl
{
    public static readonly DependencyProperty ViewTypeProperty = 
        DependencyProperty.Register(nameof(ViewType), typeof(Type), typeof(ViewLocator), 
            new PropertyMetadata(null, OnViewTypeChanged));

    public Type ViewType
    {
        get => (Type)GetValue(ViewTypeProperty);
        set => SetValue(ViewTypeProperty, value);
    }

    private static void OnViewTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ViewLocator locator)
        {
            locator.UpdateContent();
        }
    }

    private void UpdateContent()
    {
        
        
        var app = (App)Application.Current;
        var services = app.Host?.Services;
        Content = services?.GetService(ViewType);
    }
}