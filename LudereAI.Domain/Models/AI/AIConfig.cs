namespace LudereAI.Domain.Models.AI;

public class AIConfig
{
    public AIBaseConfig OpenAI { get; set; }
    public AIBaseConfig DeepSeek { get; set; }
    public AIBaseConfig Gemini { get; set; }
}