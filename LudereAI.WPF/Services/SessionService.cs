using System.IO;
using System.Security.Cryptography;
using System.Text;
using LudereAI.Shared.DTOs;
using LudereAI.WPF.Interfaces;
using Microsoft.Extensions.Logging;

namespace LudereAI.WPF.Services;

public class SessionService(ILogger<ISessionService> logger) : ISessionService
{
    private const string TokenFileName = "token.dat";

    public AccountDTO? CurrentAccount { get; private set; } = null;
    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);
    public string Token { get; private set; } = string.Empty;

    public async Task<string> GetToken()
    {
        try
        {
            if (string.IsNullOrEmpty(Token))
            {
                Token = await LoadToken();
            }
            return Token;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get token");
            RemoveToken();
            return "";
        }
    }

    public async Task SetToken(string token)
    {
        try
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException(nameof(token), "Token cannot be null or empty");
            }
                
            Token = token;
            await SaveToken(token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to set token");
        }
    }

    public void RemoveToken()
    {
        try
        {
            Token = string.Empty;
            DeleteTokenFile();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to remove token");
        }
    }
    

    public void SetCurrentAccount(AccountDTO account)
    {
        CurrentAccount = account;
    }

    public void RemoveCurrentAccount()
    {
        CurrentAccount = null;
    }

    private async Task SaveToken(string token)
    {
        try
        {
            var encryptedToken = ProtectedData.Protect(
                Encoding.UTF8.GetBytes(token),
                null,
                DataProtectionScope.CurrentUser
            );
                
            await File.WriteAllBytesAsync(TokenFileName, encryptedToken);
            logger.LogInformation("Token successfully saved");
        }
        catch (CryptographicException ex)
        {
            logger.LogError(ex, "Failed to encrypt token");
        }
        catch (IOException ex)
        {
            logger.LogError(ex, "Failed to write token to file");
        }
    }
    
    private async Task<string> LoadToken()
    {
        try
        {
            if (!File.Exists(TokenFileName))
            {
                logger.LogInformation("No token file found");
                return string.Empty;
            }

            var encryptedToken = await File.ReadAllBytesAsync(TokenFileName);
            var token = ProtectedData.Unprotect(
                encryptedToken,
                null,
                DataProtectionScope.CurrentUser
            );

            logger.LogInformation("Token successfully loaded");
            return Encoding.UTF8.GetString(token);
        }
        catch (CryptographicException ex)
        {
            logger.LogError(ex, "Failed to decrypt token");
            return "";
        }
        catch (IOException ex)
        {
            logger.LogError(ex, "Failed to read token file");
            return "";
        }
        finally
        {
            RemoveToken();
        }
    }
    
    private void DeleteTokenFile()
    {
        try
        {
            if (!File.Exists(TokenFileName)) return;
            
            File.Delete(TokenFileName);
            logger.LogInformation("Token file successfully deleted");
        }
        catch (IOException ex)
        {
            logger.LogError(ex, "Failed to delete token file");
        }
    }
    
}