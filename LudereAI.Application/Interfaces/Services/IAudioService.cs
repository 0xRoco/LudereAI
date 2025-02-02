namespace LudereAI.Application.Interfaces.Services;

public interface IAudioService
{
    Task<byte[]> GenerateAudio(string text);
}