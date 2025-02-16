using CommunityToolkit.Mvvm.ComponentModel;

namespace LudereAI.WPF.Models;

public partial class Conversation : ObservableObject
{
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private string _gameContext = string.Empty;
    [ObservableProperty] private DateTime _createdAt = DateTime.Now;
    [ObservableProperty] private DateTime _updatedAt = DateTime.Now;
    
    [ObservableProperty] private IEnumerable<Message> _messages = new List<Message>();
    
    
    public void AddMessage(Message message)
    {
        Messages = Messages.Append(message);
    }
}