using System.Net;
using LudereAI.Application.Interfaces.Gateways;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Shared;
using LudereAI.Shared.DTOs;
using Microsoft.Extensions.Logging;

namespace LudereAI.Infrastructure.Gateways;

public class AccountGateway : IAccountGateway
{
    private readonly ILogger<IAccountGateway> _logger;
    private readonly IAPIClient _apiClient;

    public AccountGateway(ILogger<IAccountGateway> logger, IAPIClient apiClient)
    {
        _logger = logger;
        _apiClient = apiClient;
    }
    
    public async Task<Result<IEnumerable<AccountDTO>, IAccountGateway.AccountOperationResult>> GetAccounts()
    {
        try
        {
            var result = await _apiClient.Get<IEnumerable<AccountDTO>>("Accounts");
            if (result is { IsSuccess: true, Data: not null })
            {
                _logger.LogInformation("Retrieved accounts successfully");
                return Result<IEnumerable<AccountDTO>, IAccountGateway.AccountOperationResult>.Success(result.Data);
            }

            _logger.LogWarning("Failed to retrieve accounts: {message}", result?.Message);
            return Result<IEnumerable<AccountDTO>, IAccountGateway.AccountOperationResult>.Error(
                IAccountGateway.AccountOperationResult.Error, 
                result?.Message ?? "An error occurred while retrieving accounts");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while retrieving accounts");
            return Result<IEnumerable<AccountDTO>, IAccountGateway.AccountOperationResult>.Error(
                IAccountGateway.AccountOperationResult.Error, 
                "An error occurred while retrieving accounts");
        }
    }

    public async Task<Result<AccountDTO, IAccountGateway.AccountOperationResult>> GetAccount(string id)
    {
        try
        {
            var result = await _apiClient.Get<AccountDTO>($"Accounts/{id}");
            if (result is { IsSuccess: true, Data: not null })
            {
                _logger.LogInformation("Retrieved account successfully");
                return Result<AccountDTO, IAccountGateway.AccountOperationResult>.Success(result.Data);
            }

            _logger.LogWarning("Failed to retrieve account: {message}", result?.Message);
            
            var errorResult = result?.StatusCode == HttpStatusCode.NotFound
                ? IAccountGateway.AccountOperationResult.NotFound 
                : IAccountGateway.AccountOperationResult.Error;
                
            return Result<AccountDTO, IAccountGateway.AccountOperationResult>.Error(
                errorResult, 
                result?.Message ?? "An error occurred while retrieving the account");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while retrieving account");
            return Result<AccountDTO, IAccountGateway.AccountOperationResult>.Error(
                IAccountGateway.AccountOperationResult.Error, 
                "An error occurred while retrieving the account");
        }
    }

    public async Task<Result<AccountDTO, IAccountGateway.AccountOperationResult>> GetAccountByUsername(string username)
    {
        try
        {
            var result = await _apiClient.Get<AccountDTO>($"Accounts/username/{username}");
            if (result is { IsSuccess: true, Data: not null })
            {
                _logger.LogInformation("Retrieved account by username successfully");
                return Result<AccountDTO, IAccountGateway.AccountOperationResult>.Success(result.Data);
            }

            _logger.LogWarning("Failed to retrieve account by username: {message}", result?.Message);
            
            var errorResult = result?.StatusCode == HttpStatusCode.NotFound 
                ? IAccountGateway.AccountOperationResult.NotFound 
                : IAccountGateway.AccountOperationResult.Error;
                
            return Result<AccountDTO, IAccountGateway.AccountOperationResult>.Error(
                errorResult, 
                result?.Message ?? "An error occurred while retrieving the account");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while retrieving account by username");
            return Result<AccountDTO, IAccountGateway.AccountOperationResult>.Error(
                IAccountGateway.AccountOperationResult.Error, 
                "An error occurred while retrieving the account");
        }
    }

    public async Task<Result<AccountDTO, IAccountGateway.AccountOperationResult>> GetAccountByEmail(string email)
    {
        try
        {
            var result = await _apiClient.Get<AccountDTO>($"Accounts/email/{email}");
            if (result is { IsSuccess: true, Data: not null })
            {
                _logger.LogInformation("Retrieved account by email successfully");
                return Result<AccountDTO, IAccountGateway.AccountOperationResult>.Success(result.Data);
            }

            _logger.LogWarning("Failed to retrieve account by email: {message}", result?.Message);
            
            var errorResult = result?.StatusCode == HttpStatusCode.NotFound 
                ? IAccountGateway.AccountOperationResult.NotFound 
                : IAccountGateway.AccountOperationResult.Error;
                
            return Result<AccountDTO, IAccountGateway.AccountOperationResult>.Error(
                errorResult, 
                result?.Message ?? "An error occurred while retrieving the account");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while retrieving account by email");
            return Result<AccountDTO, IAccountGateway.AccountOperationResult>.Error(
                IAccountGateway.AccountOperationResult.Error, 
                "An error occurred while retrieving the account");
        }
    }

    public async Task<Result<bool, IAccountGateway.AccountOperationResult>> AccountExists(string username)
    {
        try
        {
            var result = await _apiClient.Get<bool>($"Accounts/exists/{username}");
            if (result is { IsSuccess: true })
            {
                _logger.LogInformation("Checked account existence successfully");
                return Result<bool, IAccountGateway.AccountOperationResult>.Success(result.Data);
            }

            _logger.LogWarning("Failed to check account existence: {message}", result?.Message);
            return Result<bool, IAccountGateway.AccountOperationResult>.Error(
                IAccountGateway.AccountOperationResult.Error, 
                result?.Message ?? "An error occurred while checking account existence");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while checking account existence");
            return Result<bool, IAccountGateway.AccountOperationResult>.Error(
                IAccountGateway.AccountOperationResult.Error, 
                "An error occurred while checking account existence");
        }
    }

    public async Task<Result<AccountDTO, IAccountGateway.AccountOperationResult>> CreateAccount(SignUpDTO dto)
    {
        try
        {
            var result = await _apiClient.Post<AccountDTO>("Accounts", dto);
            if (result is { IsSuccess: true, Data: not null })
            {
                _logger.LogInformation("Created account successfully");
                return Result<AccountDTO, IAccountGateway.AccountOperationResult>.Success(result.Data);
            }

            _logger.LogWarning("Failed to create account: {message}", result?.Message);
            
            var errorResult = result?.StatusCode == HttpStatusCode.Conflict 
                ? IAccountGateway.AccountOperationResult.AlreadyExists 
                : IAccountGateway.AccountOperationResult.Error;
                
            return Result<AccountDTO, IAccountGateway.AccountOperationResult>.Error(
                errorResult, 
                result?.Message ?? "An error occurred while creating the account");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while creating account");
            return Result<AccountDTO, IAccountGateway.AccountOperationResult>.Error(
                IAccountGateway.AccountOperationResult.Error, 
                "An error occurred while creating the account");
        }
    }

    public async Task<Result<AccountDTO, IAccountGateway.AccountOperationResult>> CreateGuestAccount(GuestDTO dto)
    {
        try
        {
            var result = await _apiClient.Post<AccountDTO>("Accounts/guest", dto);
            if (result is { IsSuccess: true, Data: not null })
            {
                _logger.LogInformation("Created guest account successfully");
                return Result<AccountDTO, IAccountGateway.AccountOperationResult>.Success(result.Data);
            }

            _logger.LogWarning("Failed to create guest account: {message}", result?.Message);
            return Result<AccountDTO, IAccountGateway.AccountOperationResult>.Error(
                IAccountGateway.AccountOperationResult.Error, 
                result?.Message ?? "An error occurred while creating the guest account");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while creating guest account");
            return Result<AccountDTO, IAccountGateway.AccountOperationResult>.Error(
                IAccountGateway.AccountOperationResult.Error, 
                "An error occurred while creating the guest account");
        }
    }

    public async Task<Result<AccountDTO, IAccountGateway.AccountOperationResult>> UpdateAccount(string id, UpdateAccountDTO dto)
    {
        try
        {
            var result = await _apiClient.Put<AccountDTO>($"Accounts/{id}", dto);
            if (result is { IsSuccess: true, Data: not null })
            {
                _logger.LogInformation("Updated account successfully");
                return Result<AccountDTO, IAccountGateway.AccountOperationResult>.Success(result.Data);
            }

            _logger.LogWarning("Failed to update account: {message}", result?.Message);
            
            var errorResult = result?.StatusCode == HttpStatusCode.NotFound 
                ? IAccountGateway.AccountOperationResult.NotFound 
                : IAccountGateway.AccountOperationResult.Error;
                
            return Result<AccountDTO, IAccountGateway.AccountOperationResult>.Error(
                errorResult, 
                result?.Message ?? "An error occurred while updating the account");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while updating account");
            return Result<AccountDTO, IAccountGateway.AccountOperationResult>.Error(
                IAccountGateway.AccountOperationResult.Error, 
                "An error occurred while updating the account");
        }
    }

    public async Task<Result<bool, IAccountGateway.AccountOperationResult>> DeleteAccountAsync(string id)
    {
        try
        {
            var result = await _apiClient.Delete<bool>($"Accounts/{id}");
            if (result is { IsSuccess: true })
            {
                _logger.LogInformation("Deleted account successfully");
                return Result<bool, IAccountGateway.AccountOperationResult>.Success(result.Data);
            }

            _logger.LogWarning("Failed to delete account: {message}", result?.Message);
            
            var errorResult = result?.StatusCode == HttpStatusCode.NotFound 
                ? IAccountGateway.AccountOperationResult.NotFound 
                : IAccountGateway.AccountOperationResult.Error;
                
            return Result<bool, IAccountGateway.AccountOperationResult>.Error(
                errorResult, 
                result?.Message ?? "An error occurred while deleting the account");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while deleting account");
            return Result<bool, IAccountGateway.AccountOperationResult>.Error(
                IAccountGateway.AccountOperationResult.Error, 
                "An error occurred while deleting the account");
        }
    }
}