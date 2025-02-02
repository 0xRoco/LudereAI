using System.IO;
using LudereAI.WPF.Interfaces;
using Microsoft.Extensions.Logging;
using NAudio.Wave;

namespace LudereAI.WPF.Services;

public class AudioPlaybackService : IAudioPlaybackService, IDisposable
{
    private ILogger<IAudioPlaybackService> _logger;
    private IWavePlayer? _wavePlayer;
    private WaveStream? _waveStream;

    private bool _disposed;

    public AudioPlaybackService(ILogger<IAudioPlaybackService> logger)
    {
        _logger = logger;
    }

    public async Task PlayAudioAsync(byte[] audio)
    {
        if (audio.Length == 0)
        {
            _logger.LogWarning("Audio is empty");
            return;
        }
        
        StopAudioAsync();
        using var memoryStream = new MemoryStream(audio);
        _waveStream = new Mp3FileReader(memoryStream);
        _wavePlayer = new WaveOutEvent();
        _wavePlayer.Init(_waveStream);
        _wavePlayer.Play();

        while (_wavePlayer.PlaybackState == PlaybackState.Playing)
        {
            await Task.Delay(100);
        }
    }

    public void StopAudioAsync()
    {
        _wavePlayer?.Stop();
        _wavePlayer?.Dispose();
        _waveStream?.Dispose();
        
        _wavePlayer = null;
        _waveStream = null;
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        StopAudioAsync();
        
        _disposed = true;
    }
}