using LudereAI.Application.Interfaces.Repositories;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Domain.Models;
using Microsoft.Extensions.Logging;

namespace LudereAI.Infrastructure.Services;

public class WaitlistService : IWaitlistService
{
    private readonly ILogger<IWaitlistService> _logger;
    private readonly IWaitlistRepository _waitlistRepository;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateService _emailTemplateService;

    public WaitlistService(ILogger<IWaitlistService> logger, IWaitlistRepository waitlistRepository, IEmailService emailService, IEmailTemplateService emailTemplateService)
    {
        _logger = logger;
        _waitlistRepository = waitlistRepository;
        _emailService = emailService;
        _emailTemplateService = emailTemplateService;
    }


    public async Task<IEnumerable<WaitlistEntry>> GetAll()
    {
        return await _waitlistRepository.GetAll();
    }

    public async Task<WaitlistEntry?> GetByEmail(string email)
    {
        return await _waitlistRepository.GetByEmail(email);
    }

    public async Task<WaitlistEntry> JoinWaitlist(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email and name cannot be empty");
        
        var existingEntry = await _waitlistRepository.GetByEmail(email);
        if (existingEntry != null) return existingEntry;

        var entry = new WaitlistEntry
        {
            Email = email,
            JoinedDate = DateTime.UtcNow,
            Position = await _waitlistRepository.GetNextPosition(),
            IsInvited = false,
        };
        
        var result = await _waitlistRepository.Add(entry);
        
        await SendWaitlistConfirmationEmail(result);
        
        return result;
    }

    public async Task<bool> Invite(string email)
    {
        var existingEntry = await _waitlistRepository.GetByEmail(email);
        if (existingEntry is null) return false;
        
        existingEntry.IsInvited = true;
        existingEntry.InvitedDate = DateTime.UtcNow;
        await _waitlistRepository.Update(existingEntry);
        
        await SendInvitationEmailAsync(existingEntry);
        
        return true;
    }

    public async Task<bool> InviteNextBatch(int batchSize)
    {
        var nextBatch = await _waitlistRepository.GetUninvitedBatch(batchSize);
        var success = true;

        foreach (var entry in nextBatch)
        {
            try
            {
                await SendInvitationEmailAsync(entry);
                entry.IsInvited = true;
                entry.InvitedDate = DateTime.UtcNow;
                await _waitlistRepository.Update(entry);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to invite {Email}", entry.Email);
                success = false;
            }
        }
        
        return success;
    }

    public async Task<bool> RemoveFromWaitlist(string email)
    {
        var existingEntry = await _waitlistRepository.GetByEmail(email);
        if (existingEntry is null) return false;
        
        await _waitlistRepository.Delete(existingEntry.Id);
        return true;
    }
    
    private async Task SendWaitlistConfirmationEmail(WaitlistEntry entry)
    {
        const string subject = "You've joined the LudereAI waitlist!";
        var body = await _emailTemplateService.GetWaitlistConfirmationEmail(entry);
        
        await _emailService.SendEmail(entry.Email, subject, body);
    }
    
    private async Task SendInvitationEmailAsync(WaitlistEntry entry)
    {
        const string subject = "You're invited to join LudereAI!";
        var body = await _emailTemplateService.GetWaitlistInvitationEmail(entry);

        await _emailService.SendEmail(entry.Email, subject, body);
    }
}