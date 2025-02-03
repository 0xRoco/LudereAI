using System.Windows;
using LudereAI.WPF.ViewModels;

namespace LudereAI.WPF.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class ChatView : Window
{
    private readonly ChatViewModel _vm;
    public ChatView(ChatViewModel vm)
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