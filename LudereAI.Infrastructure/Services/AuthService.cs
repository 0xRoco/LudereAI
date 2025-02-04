using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LudereAI.Application.Interfaces.Repositories;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Domain.Models.Account;
using LudereAI.Shared.DTOs;
using LudereAI.Shared.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;


namespace LudereAI.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ILogger<IAuthService> _logger;
    private readonly IAccountService _accountService;
    private readonly IGuestService _guestService;
    private readonly IAccountRepository _accountRepository;
    private readonly ISecurityService _securityService;
    private readonly IConfiguration _configuration;

    public AuthService(ILogger<IAuthService> logger, 
        IAccountService accountService, 
        IAccountRepository accountRepository,
        ISecurityService securityService, 
        IConfiguration configuration, IGuestService guestService)
    {
        _logger = logger;
        _accountService = accountService;
        _accountRepository = accountRepository;
        _securityService = securityService;
        _configuration = configuration;
        _guestService = guestService;
    }

    public async Task<bool> LoginAsync(LoginDTO dto)
    {
        var account = await _accountRepository.GetByUsernameAsync(dto.Username);
        
        return account != null && _securityService.VerifyPassword(dto.Password, account.HashedPassword);
    }

    public async Task<bool> RegisterAsync(SignUpDTO dto)
    {
        var usernameExists = await _accountService.AccountExists(dto.Username);
        if (usernameExists) return false;
        
        
        var account = await _accountService.CreateAccount(dto);
        
        return account != null;
    }

    public string GenerateJWT(AccountDTO account)
    {
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"] ??
                                                                        throw new InvalidOperationException(
                                                                            "Invalid secret key")));
        var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
        var expiration = DateTime.UtcNow.AddDays(30);

        var claims = new List<Claim>()
        {
            new(JwtRegisteredClaimNames.Sub, account.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)),
            new(ClaimTypes.NameIdentifier, account.Id),
            new(ClaimTypes.Name, account.Username),
            new (ClaimTypes.Role, account.Role.ToString()),
            new("DeviceId", account.DeviceId)
        };

        var descriptor = new SecurityTokenDescriptor()
        {
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            Subject = new ClaimsIdentity(claims),
            Expires = expiration,
            SigningCredentials = credentials,
            TokenType = "Bearer"
        };

        return new JwtSecurityTokenHandler().CreateEncodedJwt(descriptor);
    }

    public async Task<bool> GuestLogin(GuestDTO dto)
    {
        var account = await _guestService.CreateGuestAccount(dto);

        return account != null;
    }

    public string GenerateGuestJWT(AccountDTO account)
    {
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"] ??
                                                                        throw new InvalidOperationException(
                                                                            "Invalid secret key")));
        var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
        var expiration = DateTime.UtcNow.AddHours(24);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, account.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)),
            new(ClaimTypes.NameIdentifier, account.Id),
            new(ClaimTypes.Name, account.Username),
            new (ClaimTypes.Role, account.Role.ToString()),
            new("DeviceId", account.DeviceId)
        };

        var descriptor = new SecurityTokenDescriptor()
        {
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            Subject = new ClaimsIdentity(claims),
            Expires = expiration,
            SigningCredentials = credentials,
            TokenType = "Bearer"
        };

        return new JwtSecurityTokenHandler().CreateEncodedJwt(descriptor);
    }
}