namespace LudereAI.WPF.Interfaces;

public interface ITokenService
{
    event EventHandler TokenInvalidated;
    Task<bool> ValidateToken();
}