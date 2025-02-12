using LudereAI.Application.Interfaces.Gateways;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Shared.DTOs;
using Microsoft.Extensions.Logging;

namespace LudereAI.Infrastructure.Gateways;

public class AuthGateway : IAuthGateway
{
    private ILogger<IAuthGateway> _logger;
    private IAPIClient _apiClient;
    
    public async Task<bool> LoginAsync(LoginDTO dto, bool rememberMe = false)
    {
        try
        {
            var result = await _apiClient.PostAsync<LoginResponseDTO>("Auth/Login", dto);
            if (result is { IsSuccess: true, Data: not null })
            {
                _logger.LogInformation("User logged in successfully");
                
                return true;
            }

            _logger.LogWarning("Login Failed: {message}", result?.Message);
            return false;

        }catch(Exception e)
        {
            _logger.LogError(e, "An error occurred while logging in");
            return false;
        }
    }

    public async Task<bool> SignUpAsync(SignUpDTO dto)
    {
        throw new NotImplementedException();
    }
}