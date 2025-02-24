using LudereAI.WPF.Models;

namespace LudereAI.WPF.Interfaces;

public interface ISettingsService
{
    AppSettings LoadSettings();
    void SaveSettings(AppSettings settings);
    void ApplySettings(AppSettings settings);
}