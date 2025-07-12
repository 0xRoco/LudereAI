namespace LudereAI.Core.Entities;

public class AIResponse
{
    public string MessageId { get; set; }
    public string ConversationId { get; set; }
    public string Message { get; set; }
    public byte[] TextToSpeech { get; set; }
}