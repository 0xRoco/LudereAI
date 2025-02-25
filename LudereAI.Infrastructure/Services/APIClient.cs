using System.Net.Http.Json;
using System.Net.Sockets;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Shared;
using Microsoft.Extensions.Logging;

namespace LudereAI.Infrastructure.Services;

public class APIClient : IAPIClient
{
    private readonly ILogger<IAPIClient> _logger;
    private readonly IHttpClientFactory _clientFactory;

    private readonly HttpClient _client;

    public APIClient(ILogger<IAPIClient> logger, IHttpClientFactory clientFactory)
    {
        _logger = logger;
        _clientFactory = clientFactory;
        
        _client = clientFactory.CreateClient("LudereAI");
    }


    public async Task<APIResult<T>?> Get<T>(string endpoint, bool auth = true)
    {
        try
        {
            if (auth)
            {
                //await SetAuthHeader();
            }

            var response = await _client.GetAsync(endpoint);
            var result = await response.Content.ReadFromJsonAsync<APIResult<T>>();
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GET request to {endpoint} failed with status code {statusCode}", endpoint, response.StatusCode);
                return result;
            }
            
            _logger.LogDebug("GET request to {endpoint} returned {result}", endpoint, result?.ToJson());
            return result;
        }
        catch (Exception e)
        {
            return HandleConnectionException<T>(e, endpoint, "GET");
        }
    }

    public async Task<APIResult<T>?> Post<T>(string endpoint, object data, bool auth = true)
    {
        try
        {
            if (auth)
            {
                //await SetAuthHeader();
            }

            var response = await _client.PostAsJsonAsync(endpoint, data);
            var result = await response.Content.ReadFromJsonAsync<APIResult<T>>();
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("POST request to {endpoint} failed with status code {statusCode}", endpoint, response.StatusCode);
                return result;
            }
            
            _logger.LogDebug("POST request to {endpoint} returned {result}", endpoint, result?.ToJson());
            return result;
        }
        catch (Exception e)
        {
            return HandleConnectionException<T>(e, endpoint, "POST");
        }
    }

    public async Task<APIResult<T>?> Put<T>(string endpoint, object data, bool auth = true)
    {
        try
        {
            if (auth)
            {
                //await SetAuthHeader();
            }

            var response = await _client.PutAsJsonAsync(endpoint, data);
            var result = await response.Content.ReadFromJsonAsync<APIResult<T>>();
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("PUT request to {endpoint} failed with status code {statusCode}", endpoint, response.StatusCode);
                return result;
            }
            
            _logger.LogDebug("PUT request to {endpoint} returned {result}", endpoint, result?.ToJson());
            return result;
        }
        catch (Exception e)
        {
            return HandleConnectionException<T>(e, endpoint, "PUT");
        }
    }

    public async Task<APIResult<T>?> Delete<T>(string endpoint, bool auth = true)
    {
        try
        {
            if (auth)
            {
                //await SetAuthHeader();
            }

            var response = await _client.DeleteAsync(endpoint);
            var result = await response.Content.ReadFromJsonAsync<APIResult<T>>();
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("DELETE request to {endpoint} failed with status code {statusCode}", endpoint, response.StatusCode);
                return result;
            }
            
            _logger.LogDebug("DELETE request to {endpoint} returned {result}", endpoint, result?.ToJson());
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
                    _logger.LogWarning("API server is offline or not accepting connections at {request} {endpoint}", request, endpoint);
                    return new APIResult<T> 
                    { 
                        IsSuccess = false, 
                        Message = "Unable to connect to the server. Please check your connection and try again." 
                    };
                case SocketError.TimedOut:
                    _logger.LogWarning("Connection to API server timed out at {request} {endpoint}", request, endpoint);
                    return new APIResult<T> 
                    { 
                        IsSuccess = false, 
                        Message = "Connection timed out. Please try again." 
                    };
            }
        }
        
        _logger.LogError(ex, "Failed to execute {request} request to {endpoint}", request, endpoint);
        return new APIResult<T> 
        { 
            IsSuccess = false, 
            Message = "An unexpected error occurred. Please try again later." 
        };
    }
}