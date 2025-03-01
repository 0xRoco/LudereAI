using LudereAI.WPF.Interfaces;
using LudereAI.WPF.Views;
using Microsoft.Extensions.Logging;

namespace LudereAI.WPF.Services;

public class OverlayService : IOverlayService
{
    private readonly ILogger<IOverlayService> _logger;
    private readonly INavigationService _navigationService;
    
    public bool IsOverlayVisible { get; private set; }
    public event Action<bool>? OnOverlayVisibilityChanged;

    public OverlayService(ILogger<IOverlayService> logger, INavigationService navigationService)
    {
        _logger = logger;
        _navigationService = navigationService;
    }
    
    public void ShowOverlay()
    {
        if (IsOverlayVisible) return;
        
        _navigationService.ShowWindow<OverlayView>(isMainWindow: false);
        
        IsOverlayVisible = true;
        OnOverlayVisibilityChanged?.Invoke(IsOverlayVisible);
        
        _logger.LogInformation("Overlay shown");
    }

    public void HideOverlay()
    {
        if (!IsOverlayVisible) return;
        
        _navigationService.CloseWindow<OverlayView>();
        
        IsOverlayVisible = false;
        OnOverlayVisibilityChanged?.Invoke(IsOverlayVisible);
        
        _logger.LogInformation("Overlay hidden");
    }

    public void ToggleOverlay()
    {
        if (IsOverlayVisible)
        {
            HideOverlay();
        }
        else
        {
            ShowOverlay();
        }
    }
}