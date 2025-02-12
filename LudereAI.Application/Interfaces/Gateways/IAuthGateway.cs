using LudereAI.Shared.DTOs;

namespace LudereAI.Application.Interfaces.Gateways;

public interface IAuthGateway
{
    Task<bool> LoginAsync(LoginDTO dto, bool rememberMe = false);
    Task<bool> SignUpAsync(SignUpDTO dto);
}