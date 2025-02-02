namespace LudereAI.WPF.Interfaces;

public interface IAudioPlaybackService
{
    Task PlayAudioAsync(byte[] audio);
    void StopAudioAsync();
}