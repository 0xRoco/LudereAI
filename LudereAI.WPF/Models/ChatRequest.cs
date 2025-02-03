namespace LudereAI.WPF.Models;

public class ChatRequest
{
    public required string Message { get; set; }
    public string? ConversationId { get; set; }
    public string? GameContext { get; set; }
    public WindowInfo? Window { get; set; }
}