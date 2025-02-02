using BCrypt.Net;

namespace LudereAI.Application.Interfaces.Services;

public interface ISecurityService
{
    string HashPassword(string password, int cost = 9, HashType hashType = HashType.SHA512);
    bool VerifyPassword(string password, string passwordHash, HashType hashType = HashType.SHA512);
}