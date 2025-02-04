using LudereAI.Shared.DTOs;

namespace LudereAI.WPF.Interfaces;

public interface IAuthService
{
    Task<bool> LoginAsync(LoginDTO dto, bool rememberMe = false);
    Task<bool> GuestLoginAsync();
    Task<bool> SignUpAsync(SignUpDTO dto);
    Task<bool> LogoutAsync();
}