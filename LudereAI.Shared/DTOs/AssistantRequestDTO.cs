namespace LudereAI.Shared.DTOs;

public class AssistantRequestDTO
{
    public string ConversationId { get; set; }
    public string Message { get; set; }
    public string Screenshot { get; set; }
    public string GameContext { get; set; }
    
    public bool IsScreenshotValid()
    {
        if (string.IsNullOrWhiteSpace(Screenshot)) return true;
        Uri.TryCreate(Screenshot, UriKind.Absolute, out var uri);
        return uri != null && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
    
    public bool IsMessageValid()
    {
        return !string.IsNullOrWhiteSpace(Message);
    }
    
    public bool IsConversationIdValid()
    {
        return !string.IsNullOrWhiteSpace(ConversationId);
    }
    
    public bool IsGameContextValid()
    {
        return !string.IsNullOrWhiteSpace(GameContext);
    }
}