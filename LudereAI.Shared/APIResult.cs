using System.Net;

namespace LudereAI.Shared;

public class APIResult<T>(bool success, HttpStatusCode statusCode, string message, T? data)
{
    public bool IsSuccess { get; set; } = success;
    public HttpStatusCode StatusCode { get; set; } = statusCode;
    public string Message { get; set; } = message;
    public T? Data { get; set; } = data;

    public APIResult() : this(default, default, "", default)
    {
        
    }
    
    public static APIResult<T> Success(string message = "", T? data = default)=> new(true, HttpStatusCode.OK, message, data);
    public static APIResult<T> Error(HttpStatusCode statusCode, string message = "", T? data = default) => new(false, statusCode, message, data);
    public static APIResult<T> Custom(bool success, HttpStatusCode statusCode, string message = "", T? data = default) => new(success, statusCode, message, data);
}