using LudereAI.Shared.DTOs;
using LudereAI.WPF.Interfaces;
using LudereAI.WPF.Views;
using Microsoft.Extensions.Logging;

namespace LudereAI.WPF.Services;

public class AuthService(ILogger<IAuthService> logger, 
    IAPIClient apiClient,
    ISessionService sessionService,
    INavigationService navigationService,
    IGameService gameService) : IAuthService
{
    public async Task<bool> LoginAsync(LoginDTO dto, bool rememberMe = false)
    {
        try
        {
            var result = await apiClient.PostAsync<LoginResponseDTO>("Auth/Login", dto);
            if (result is { IsSuccess: true, Data: not null })
            {
                await sessionService.SetToken(result.Data.Token);
                
                logger.LogInformation("User logged in successfully");
                
                sessionService.SetCurrentAccount(result.Data.Account);
                
                return true;
            }

            logger.LogWarning("Login Failed: {message}", result?.Message);
            return false;

        }catch(Exception e)
        {
            logger.LogError(e, "An error occurred while logging in");
            return false;
        }
    }

    public async Task<bool> GuestLoginAsync()
    {
        try
        {
            var guest = new GuestDTO { DeviceId = GetDeviceId() };
            var result = await apiClient.PostAsync<LoginResponseDTO>("Auth/GuestLogin", guest);
            if (result is { IsSuccess: true, Data: not null })
            {
                await sessionService.SetToken(result.Data.Token);
                logger.LogInformation("User logged in as guest successfully");
                
                sessionService.SetCurrentAccount(result.Data.Account);
                
                return true;
            }
            
            logger.LogWarning("Guest Login Failed: {message}", result?.Message);
            
            return false;
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while logging in as guest");
            return false;
        }
    }

    public async Task<bool> SignUpAsync(SignUpDTO dto)
    {
        try
        {
            var result = await apiClient.PostAsync<string>("Auth/SignUp", dto);
            if (result?.IsSuccess == true)
            {
                logger.LogInformation("User signed up successfully");
                return true;
            }

            logger.LogWarning("Sign Up Failed: {message}", result?.Message);
            return false;

        }catch(Exception e)
        {
            logger.LogError(e, "An error occurred while signing up");
            return false;
        }
    }

    public async Task<bool> LogoutAsync()
    {
        try
        {
            //await apiClient.PostAsync<bool>("Auth/Logout", new {});

            await gameService.StopScanning();
            sessionService.RemoveCurrentAccount();
            sessionService.RemoveToken(); 
            navigationService.ShowWindow<AuthView>();
            navigationService.CloseAllWindowsButMain();
            logger.LogInformation("User logged out");
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while logging out");
            sessionService.RemoveToken();
            return false;
        }
    }
    
    private string GetDeviceId()
    {
        return Environment.MachineName + "_" + GetMachineGuid();
    }

    private string? GetMachineGuid()
    {
        const string location = @"SOFTWARE\Microsoft\Cryptography";
        const string name = "MachineGuid";

        using var localMachine = Microsoft.Win32.RegistryKey.OpenBaseKey(
            Microsoft.Win32.RegistryHive.LocalMachine, 
            Microsoft.Win32.RegistryView.Registry64);
        using var rk = localMachine.OpenSubKey(location);
        if (rk == null)
            throw new KeyNotFoundException($"Key Not Found: {location}");

        var machineGuid = rk.GetValue(name);
        if (machineGuid == null)
            throw new IndexOutOfRangeException($"Index Not Found: {name}");

        return machineGuid.ToString() ?? throw new NullReferenceException($"Null Reference: {name}");
    }
}