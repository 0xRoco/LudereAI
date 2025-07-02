using LudereAI.Shared.Models;

namespace LudereAI.Core.Interfaces.Services;

public interface ISettingsService
{
    event Action<AppSettings> OnSettingsApplied;
    
    Task<AppSettings> LoadSettings();
    Task SaveSettings(AppSettings settings, CancellationToken ct);
    void ApplySettings(AppSettings settings);
}