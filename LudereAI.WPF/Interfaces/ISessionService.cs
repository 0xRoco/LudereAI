using LudereAI.Shared.DTOs;

namespace LudereAI.WPF.Interfaces;

public interface ISessionService
{
    public AccountDTO? CurrentAccount { get; }
    public bool IsAuthenticated { get; }
    public string Token { get; }
    Task<string> GetToken();
    Task SetToken(string token);
    void RemoveToken(); 
    
    void SetCurrentAccount(AccountDTO account);
    void RemoveCurrentAccount();
}