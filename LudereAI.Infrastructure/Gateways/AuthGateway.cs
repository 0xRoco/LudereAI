using LudereAI.Application.Interfaces.Gateways;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Shared;
using LudereAI.Shared.DTOs;
using Microsoft.Extensions.Logging;

namespace LudereAI.Infrastructure.Gateways;

public class AuthGateway : IAuthGateway
{
    private ILogger<IAuthGateway> _logger;
    private IAPIClient _apiClient;

    public AuthGateway(ILogger<IAuthGateway> logger, IAPIClient apiClient)
    {
        _logger = logger;
        _apiClient = apiClient;
    }

    public async Task<Result<bool, IAuthGateway.LoginResult>> LoginAsync(LoginDTO dto, bool rememberMe = false)
    {
        try
        {
            var result = await _apiClient.PostAsync<LoginResponseDTO>("Auth/Login", dto);
            if (result is { IsSuccess: true, Data: not null })
            {
                _logger.LogInformation("User logged in successfully");
                
                return Result<bool, IAuthGateway.LoginResult>.Success(true);
            }

            _logger.LogWarning("Login Failed: {message}", result?.Message);
            return result is { Message: "Invalid login attempt" }
                ? Result<bool, IAuthGateway.LoginResult>.Error(IAuthGateway.LoginResult.InvalidCredentials, "Invalid login attempt")
                : Result<bool, IAuthGateway.LoginResult>.Error(IAuthGateway.LoginResult.Error, result?.Message ?? "An error occurred while logging in");
            
        }catch(Exception e)
        {
            _logger.LogError(e, "An error occurred while logging in");
            return Result<bool, IAuthGateway.LoginResult>.Error(IAuthGateway.LoginResult.Error, "An error occurred while logging in");
        }
    }

    public async Task<Result<bool, IAuthGateway.SignUpResult>> SignUpAsync(SignUpDTO dto)
    {
        throw new NotImplementedException();
    }
}