using LudereAI.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;
using NAudio.Wave;

namespace LudereAI.Services.Services;

public class AudioService(ILogger<IAudioService> logger) : IAudioService
{
    private WaveOutEvent? _waveOutEvent;
    private WaveStream? _waveStream;

    public async Task PlayAudioAsync(byte[] audio)
    {
        if (audio.Length == 0)
        {
            logger.LogWarning("Audio is empty");
            return;
        }
        
        StopAudioAsync();
        using var memoryStream = new MemoryStream(audio);
        _waveStream = new WaveFileReader(memoryStream);
        _waveOutEvent = new WaveOutEvent();
        _waveOutEvent.Init(_waveStream);
        _waveOutEvent.Play();

        while (_waveOutEvent.PlaybackState == PlaybackState.Playing)
        {
            await Task.Delay(100);
        }
    }

    public void StopAudioAsync()
    {
        _waveOutEvent?.Stop();
        _waveOutEvent?.Dispose();
        _waveStream?.Dispose();
        
        _waveOutEvent = null;
        _waveStream = null;
    }
}