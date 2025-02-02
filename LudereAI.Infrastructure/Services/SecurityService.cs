using BCrypt.Net;
using LudereAI.Application.Interfaces.Services;

namespace LudereAI.Infrastructure.Services;

public class SecurityService : ISecurityService
{
    public string HashPassword(string password, int cost = 9, HashType hashType = HashType.SHA512)
    {
        var hash = BC.EnhancedHashPassword(password, cost, hashType);
        return hash;
    }

    public bool VerifyPassword(string password, string passwordHash, HashType hashType = HashType.SHA512)
    {
        return BC.EnhancedVerify(password, passwordHash, hashType);
    }
}