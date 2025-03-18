using LudereAI.Shared;
using LudereAI.Shared.DTOs;

namespace LudereAI.Application.Interfaces.Gateways;

public interface IAuthGateway
{
    Task<Result<LoginResponseDTO, LoginResult>> LoginAsync(LoginDTO dto, bool rememberMe = false);
    Task<Result<bool, SignUpResult>> SignUpAsync(SignUpDTO dto);
    
    public enum LoginResult
    {
        Success,
        InvalidCredentials,
        Error
    }
    
    public enum SignUpResult
    {
        Success,
        UsernameTaken,
        Error
    }
}