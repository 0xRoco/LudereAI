namespace LudereAI.Core.Interfaces.Services;

public interface IAudioService
{
    Task PlayAudioAsync(byte[] audio);
    void StopAudioAsync();
}