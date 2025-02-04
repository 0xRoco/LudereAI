using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using LudereAI.Shared;
using LudereAI.Shared.DTOs;
using LudereAI.WPF.Interfaces;
using Microsoft.Extensions.Logging;

namespace LudereAI.WPF.Services;

public class TokenService(ILogger<ITokenService> logger,
    ISessionService sessionService,
    IHttpClientFactory clientFactory) : ITokenService
{
    private readonly HttpClient _httpClient = clientFactory.CreateClient("LudereAI");
    
    public event EventHandler? TokenInvalidated;

    public async Task<AccountDTO?> ValidateToken()
    {
        try
        {
            var token = sessionService.Token;
            if (!sessionService.IsAuthenticated || string.IsNullOrEmpty(token))
            {
                return null;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync("Auth/ValidateToken");
            var result = await response.Content.ReadFromJsonAsync<APIResult<AccountDTO>>();

            if (result?.IsSuccess == true)
            {
                return result.Data;
            }

            logger.LogWarning("Token validation failed: {message}", result?.Message);
            OnTokenInvalidated();
            return null;
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while validating token");
            OnTokenInvalidated();
            return null;
        }
    }

    protected virtual void OnTokenInvalidated()
    {
        logger.LogWarning("Token invalidated");
        TokenInvalidated?.Invoke(this, EventArgs.Empty);
    }
}