using LudereAI.Shared.Models;
using AppSettings = LudereAI.Core.Entities.AppSettings;

namespace LudereAI.Core.Interfaces.Services;

public interface ISettingsService
{
    event Action<AppSettings> OnSettingsApplied;
    
    AppSettings Settings { get; }
    Task<AppSettings> LoadSettings();
    Task SaveSettings(AppSettings settings, CancellationToken ct);
    void ApplySettings(AppSettings settings);
}