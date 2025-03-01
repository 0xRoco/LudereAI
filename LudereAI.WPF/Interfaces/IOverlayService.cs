namespace LudereAI.WPF.Interfaces;

public interface IOverlayService
{
    bool IsOverlayVisible { get; }
    
    event Action<bool> OnOverlayVisibilityChanged;
    
    void ShowOverlay();
    void HideOverlay();
    
    void ToggleOverlay();
}