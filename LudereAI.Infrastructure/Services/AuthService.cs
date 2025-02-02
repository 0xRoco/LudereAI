using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LudereAI.Application.Interfaces.Repositories;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Shared.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;


namespace LudereAI.Infrastructure.Services;

public class AuthService(ILogger<IAuthService> logger, 
    IAccountService accountService, 
    IAccountRepository accountRepository,
    ISecurityService securityService, 
    IConfiguration configuration) : IAuthService
{
    public async Task<bool> LoginAsync(LoginDTO dto)
    {
        var account = await accountRepository.GetByUsernameAsync(dto.Username);
        
        return account != null && securityService.VerifyPassword(dto.Password, account.HashedPassword);
    }

    public async Task<bool> RegisterAsync(SignUpDTO dto)
    {
        var usernameExists = await accountService.AccountExists(dto.Username);
        if (usernameExists) return false;
        
        
        var account = await accountService.CreateAccount(dto);
        
        return account != null;
    }

    public string GenerateJWT(AccountDTO account)
    {
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"] ??
                                                                        throw new InvalidOperationException(
                                                                            "Invalid secret key")));
        var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
        var expiration = DateTime.UtcNow.AddHours(6);

        var claims = new List<Claim>()
        {
            new(JwtRegisteredClaimNames.Sub, account.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)),
            new(ClaimTypes.NameIdentifier, account.Id),
            new(ClaimTypes.Name, account.Username),};

        var descriptor = new SecurityTokenDescriptor()
        {
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"],
            Subject = new ClaimsIdentity(claims),
            Expires = expiration,
            SigningCredentials = credentials,
            TokenType = "Bearer"
        };

        return new JwtSecurityTokenHandler().CreateEncodedJwt(descriptor);
    }
}