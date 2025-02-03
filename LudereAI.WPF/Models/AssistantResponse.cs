namespace LudereAI.WPF.Models;

public class AssistantResponse
{
    public string? Message { get; set; }
    public byte[] Audio { get; init; } = [];
}