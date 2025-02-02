using LudereAI.Shared;

namespace LudereAI.WPF.Interfaces;

public interface IAPIClient
{
    Task<APIResult<T>?> GetAsync<T>(string endpoint);
    Task<APIResult<T>?> PostAsync<T>(string endpoint, object data);
    Task<APIResult<T>?> PutAsync<T>(string endpoint, object data);
    Task<APIResult<T>?> DeleteAsync<T>(string endpoint);
}