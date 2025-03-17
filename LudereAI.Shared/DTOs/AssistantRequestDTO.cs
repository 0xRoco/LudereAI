namespace LudereAI.Shared.DTOs;

public class AssistantRequestDTO
{
    public string ConversationId { get; set; }
    public string Message { get; set; }
    public string Screenshot { get; set; }
    public string GameContext { get; set; }
    
    public bool TextToSpeechEnabled { get; set; } = true;
    
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