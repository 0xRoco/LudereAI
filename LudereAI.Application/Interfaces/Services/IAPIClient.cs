using LudereAI.Shared;

namespace LudereAI.Application.Interfaces.Services;

public interface IAPIClient
{
    Task<APIResult<T>?> GetAsync<T>(string endpoint, bool auth = true);
    Task<APIResult<T>?> PostAsync<T>(string endpoint, object data, bool auth = true);
    Task<APIResult<T>?> PutAsync<T>(string endpoint, object data, bool auth = true);
    Task<APIResult<T>?> DeleteAsync<T>(string endpoint, bool auth = true);
}