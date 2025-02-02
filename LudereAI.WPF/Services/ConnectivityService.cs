using LudereAI.WPF.Interfaces;
using Microsoft.Extensions.Logging;

namespace LudereAI.WPF.Services;

public class ConnectivityService(ILogger<IConnectivityService> logger,
    IAPIClient apiClient) : IConnectivityService
{
    private readonly CancellationTokenSource _cts = new();
    private bool _isChecking;
    private bool _isConnected;
    private const int CheckInterval = 30;

    public event EventHandler<bool>? OnConnectivityChanged;
    
    public async Task StartConnectivityCheck()
    {
        if (_isChecking)
        {
            return;
        }

        _isChecking = true;

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(CheckInterval));
        while (await timer.WaitForNextTickAsync(_cts.Token))
        {
            await CheckConnectivity();
        }
    }
    
    public async Task CheckConnectivity()
    {
        try
        {
            if (!_isChecking) return;
            
            var result = await apiClient.GetAsync<bool>("health");
            _isConnected = result is { IsSuccess: true, Data: true };

            OnConnectivityChanged?.Invoke(this, _isConnected);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to check API connectivity");
            
            _isConnected = false;
            OnConnectivityChanged?.Invoke(this, _isConnected);
        }
    }

}