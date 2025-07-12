namespace LudereAI.Core.Interfaces.Services;

public interface ITextToSpeechService
{
    Task<byte[]> GenerateTTS(string text);
}