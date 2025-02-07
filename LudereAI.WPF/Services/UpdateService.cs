using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows;
using LudereAI.Shared.DTOs;
using LudereAI.WPF.Interfaces;
using Microsoft.Extensions.Logging;
using Exception = System.Exception;

namespace LudereAI.WPF.Services;

public class UpdateService : IUpdateService
{
    private readonly ILogger<IUpdateService> _logger;
    private readonly IAPIClient _apiClient;
    private UpdateInfoDTO? _cachedUpdateInfo;
    private readonly string _updateDirectory;

    public UpdateService(ILogger<IUpdateService> logger, IAPIClient apiClient)
    {
        _logger = logger;
        _apiClient = apiClient;
        _updateDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "LudereAI", "Updates");
    }

    public async Task CheckForUpdatesAsync()
    {
        try
        {
            if (await IsUpdateAvailableAsync())
            {
                _logger.LogInformation("Update available {Version} Critical: {IsCritical}", 
                    _cachedUpdateInfo?.Version, _cachedUpdateInfo?.IsCritical);

                var message = _cachedUpdateInfo?.IsCritical == true
                    ? "A critical update for LudereAI is available"
                    : "A new version of LudereAI is available";

                var result = MessageBox.Show(
                    $"{message}: {_cachedUpdateInfo?.Version}\n\n" +
                    $"Changelog:\n{_cachedUpdateInfo?.Changelog}\n\n" +
                    "Would you like to download and install the update now?",
                    "Update Available",
                    MessageBoxButton.YesNo,
                    _cachedUpdateInfo?.IsCritical == true 
                        ? MessageBoxImage.Warning 
                        : MessageBoxImage.Information
                );

                if (result == MessageBoxResult.Yes)
                {
                    await DownloadAndInstallUpdate();
                }
                else if (_cachedUpdateInfo?.IsCritical == true)
                {
                    Application.Current.Shutdown();
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to check for updates");
            throw;
        }
    }

    public async Task<bool> IsUpdateAvailableAsync()
    {
        try
        {
            var updateInfo = await GetUpdateInfoAsync();
            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
            
            return updateInfo.Version > currentVersion;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to check if update is available");
            return false;
        }
    }

    public async Task<UpdateInfoDTO> GetUpdateInfoAsync()
    {
        try
        {
            if (_cachedUpdateInfo != null) return _cachedUpdateInfo;

            var result = await _apiClient.GetAsync<UpdateInfoDTO>("update");

            if (result is { IsSuccess: true, Data: not null })
            {
                _cachedUpdateInfo = result.Data;
                return result.Data;
            }

            _logger.LogWarning("Failed to get update info: {Error}", result?.Message);
            throw new Exception("Failed to retrieve update info");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to get update info");
            throw;
        }
    }

    private async Task DownloadAndInstallUpdate()
    {
        try
        {
            if (_cachedUpdateInfo == null) return;

            Directory.CreateDirectory(_updateDirectory);
            var installerPath = Path.Combine(_updateDirectory,
                Path.GetFileName(_cachedUpdateInfo.InstallerName));

            using var client = new HttpClient();
            var response = await client.GetAsync(_cachedUpdateInfo.DownloadUrl);
            await using (var fs = new FileStream(installerPath, FileMode.Create))
            {
                await response.Content.CopyToAsync(fs);
                await fs.FlushAsync();
            }

            using (var sh256 = SHA256.Create())
            await using (var stream = File.OpenRead(installerPath))
            {
                var hash = Convert.ToHexStringLower(await sh256.ComputeHashAsync(stream));

                if (hash != _cachedUpdateInfo.Hash)
                {
                    throw new Exception("Downloaded file hash does not match expected hash");
                }
            }


            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = installerPath,
                    UseShellExecute = true
                }
            };

            GC.Collect();
            GC.WaitForPendingFinalizers();

            Application.Current.Exit += (sender, args) => process.Start();

            Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download and install update");
            MessageBox.Show(
                "Failed to download and install update. Please try again later.",
                "Update Failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }
    }
}