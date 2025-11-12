using System.Windows;
using LudereAI.WPF.ViewModels;

namespace LudereAI.WPF.Views;

public partial class OverlayView : Window
{
    private readonly OverlayViewModel _vm;
    
    public OverlayView(OverlayViewModel vm)
    {
        InitializeComponent();
        
        DataContext = _vm = vm;

        vm.OnMessageUpdated += MessagesOnCollectionChanged;

        MessageInput.Focus();
    }

    private void MessagesOnCollectionChanged()
    {
        ChatScroll.ScrollToEnd();
    }

    protected override void OnClosed(EventArgs e)
    {
        _vm.OnMessageUpdated -= MessagesOnCollectionChanged;
        base.OnClosed(e);
    }
}