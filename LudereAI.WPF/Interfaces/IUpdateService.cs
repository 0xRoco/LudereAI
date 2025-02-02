using LudereAI.Shared.DTOs;

namespace LudereAI.WPF.Interfaces;

public interface IUpdateService
{
    Task CheckForUpdatesAsync();
    Task<bool> IsUpdateAvailableAsync();
    Task<UpdateInfoDTO> GetUpdateInfoAsync();
}