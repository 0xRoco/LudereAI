using CommunityToolkit.Mvvm.ComponentModel;
using LudereAI.Shared.Enums;

namespace LudereAI.WPF.Models;

public partial class Message : ObservableObject
{
    [ObservableProperty] private string _id = string.Empty;
    [ObservableProperty] private string _conversationId = string.Empty;
    [ObservableProperty] private string _content = string.Empty;
    [ObservableProperty] private byte[] _audio = [];
    [ObservableProperty] private MessageRole _role;
    [ObservableProperty] private DateTime _createdAt = DateTime.Now;
}