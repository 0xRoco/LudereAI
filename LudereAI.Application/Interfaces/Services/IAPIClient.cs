using LudereAI.Shared;

namespace LudereAI.Application.Interfaces.Services;

public interface IAPIClient
{
    Task<APIResult<T>?> Get<T>(string endpoint, bool auth = true);
    Task<APIResult<T>?> Post<T>(string endpoint, object data, bool auth = true);
    Task<APIResult<T>?> Put<T>(string endpoint, object data, bool auth = true);
    Task<APIResult<T>?> Delete<T>(string endpoint, bool auth = true);
}