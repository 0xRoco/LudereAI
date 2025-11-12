using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LudereAI.WPF.Models;

public partial class ConversationModel : ObservableObject
{
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private string _gameContext = string.Empty;
    [ObservableProperty] private DateTime _createdAt = DateTime.Now;
    [ObservableProperty] private DateTime _updatedAt = DateTime.Now;

    [ObservableProperty] private ObservableCollection<MessageModel> _messages = [];
}