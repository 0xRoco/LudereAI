using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Sockets;
using LudereAI.Shared;
using LudereAI.WPF.Interfaces;
using Microsoft.Extensions.Logging;

namespace LudereAI.WPF.Services;

public class APIClient(ILogger<IAPIClient> logger,
    IHttpClientFactory clientFactory,
    ITokenService tokenService,
    ISessionService sessionService) : IAPIClient
{
    
    private readonly HttpClient _client = clientFactory.CreateClient("LudereAI");

    private async Task SetAuthHeader()
    {
        if (sessionService.IsAuthenticated)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessionService.Token);
            
            await tokenService.ValidateToken();
        }
    }
    
    
    public async Task<APIResult<T>?> GetAsync<T>(string endpoint)
    {
        try
        {
            await SetAuthHeader();
            var response = await _client.GetAsync(endpoint);
            var result = await response.Content.ReadFromJsonAsync<APIResult<T>>();
            
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("GET request to {endpoint} failed with status code {statusCode}", endpoint, response.StatusCode);
                return result;
            }
            
            logger.LogDebug("GET request to {endpoint} returned {result}", endpoint, result?.ToJson());
            return result;
        }
        catch (Exception e)
        {
            return HandleConnectionException<T>(e, endpoint, "GET");
        }
    }

    public async Task<APIResult<T>?> PostAsync<T>(string endpoint, object data)
    {
        try
        {
            await SetAuthHeader();
            var response = await _client.PostAsJsonAsync(endpoint, data);
            var result = await response.Content.ReadFromJsonAsync<APIResult<T>>();
            
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("POST request to {endpoint} failed with status code {statusCode}", endpoint, response.StatusCode);
                return result;
            }
            
            logger.LogDebug("POST request to {endpoint} returned {result}", endpoint, result?.ToJson());
            return result;
        }
        catch (Exception e)
        {
            return HandleConnectionException<T>(e, endpoint, "POST");
        }
    }

    public async Task<APIResult<T>?> PutAsync<T>(string endpoint, object data)
    {
        try
        {
            await SetAuthHeader();
            var response = await _client.PutAsJsonAsync(endpoint, data);
            var result = await response.Content.ReadFromJsonAsync<APIResult<T>>();
            
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("PUT request to {endpoint} failed with status code {statusCode}", endpoint, response.StatusCode);
                return result;
            }
            
            logger.LogDebug("PUT request to {endpoint} returned {result}", endpoint, result?.ToJson());
            return result;
        }
        catch (Exception e)
        {
            return HandleConnectionException<T>(e, endpoint, "PUT");
        }
    }

    public async Task<APIResult<T>?> DeleteAsync<T>(string endpoint)
    {
        try
        {
            await SetAuthHeader();
            var response = await _client.DeleteAsync(endpoint);
            var result = await response.Content.ReadFromJsonAsync<APIResult<T>>();
            
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("DELETE request to {endpoint} failed with status code {statusCode}", endpoint, response.StatusCode);
                return result;
            }
            
            logger.LogDebug("DELETE request to {endpoint} returned {result}", endpoint, result?.ToJson());
            return result;
        }
        catch (Exception e)
        {
            return HandleConnectionException<T>(e, endpoint, "DELETE");
        }
    }
    
    private APIResult<T>? HandleConnectionException<T>(Exception ex, string endpoint, string request)
    {
        if (ex is HttpRequestException { InnerException: SocketException socketEx })
        {
            switch (socketEx.SocketErrorCode)
            {
                case SocketError.ConnectionRefused:
                    logger.LogWarning("API server is offline or not accepting connections at {request} {endpoint}", request, endpoint);
                    return new APIResult<T> 
                    { 
                        IsSuccess = false, 
                        Message = "Unable to connect to the server. Please check your connection and try again." 
                    };
                case SocketError.TimedOut:
                    logger.LogWarning("Connection to API server timed out at {request} {endpoint}", request, endpoint);
                    return new APIResult<T> 
                    { 
                        IsSuccess = false, 
                        Message = "Connection timed out. Please try again." 
                    };
            }
        }
        
        logger.LogError(ex, "Failed to execute {request} request to {endpoint}", request, endpoint);
        return new APIResult<T> 
        { 
            IsSuccess = false, 
            Message = "An unexpected error occurred. Please try again later." 
        };
    }
}