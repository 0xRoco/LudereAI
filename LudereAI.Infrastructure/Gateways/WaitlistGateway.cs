using LudereAI.Application.Interfaces.Gateways;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Domain.Models;
using LudereAI.Shared.DTOs;
using LudereAI.Shared.DTOs.Waitlist;
using Microsoft.Extensions.Logging;

namespace LudereAI.Infrastructure.Gateways;

public class WaitlistGateway : IWaitlistGateway
{
    private ILogger<WaitlistGateway> _logger;
    private IAPIClient _apiClient;

    public WaitlistGateway(IAPIClient apiClient, ILogger<WaitlistGateway> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<IEnumerable<WaitlistEntry>> GetAll()
    {
        throw new NotImplementedException();
    }

    public async Task<WaitlistEntry?> GetByEmail(string email)
    {
        throw new NotImplementedException();
    }

    public async Task<JoinedWaitlistDTO> JoinWaitlist(string email)
    {
        try
        {
            var result = await _apiClient.Post<JoinedWaitlistDTO>("waitlist/join", new JoinWaitlistDTO {Email = email});
            if (result is { IsSuccess: true, Data: not null })
            {
                return result.Data;
            }

            return result != null
                ? throw new Exception(result.Message)
                : throw new Exception("An error occurred while joining the waitlist");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while joining the waitlist");
            throw;
        }
    }

    public async Task<bool> Invite(string email)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> InviteNextBatch(int batchSize)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> RemoveFromWaitlist(string email)
    {
        throw new NotImplementedException();
    }
}