namespace LudereAI.Shared;

public class Result<T, TStatus> where TStatus : Enum?
{
    public T? Value { get; set; }
    public TStatus Status { get; set; }
    public string Message { get; set; }
    public bool IsSuccessful { get; set; }
    
    private Result(T? value, TStatus status, string message, bool isSuccessful)
    {
        Value = value;
        Status = status;
        Message = message;
        IsSuccessful = isSuccessful;
    }
    
    public static Result<T, TStatus?> Success(T value, string message = "") => new(value, default, message, true);
    public static Result<T, TStatus?> Error(TStatus status, string message = "") => new(default, status, message, false);
    public static Result<T, TStatus?> Custom(T value, TStatus status, string message = "") => new(value, status, message, status is not null);
    
}