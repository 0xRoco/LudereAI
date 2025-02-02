namespace LudereAI.Domain.Models;

public class AIResponse
{ 
    public string MessageId { get; set; }
    public string ConversationId { get; set; }
    public string Message { get; set; }
    public byte[] ttsAudio { get; set; }
    public AIResponse(){}
}