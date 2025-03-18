using CommunityToolkit.HighPerformance;
using LudereAI.Application.Interfaces.Services;
using LudereAI.Domain.Models;
using LudereAI.Shared.DTOs;

namespace LudereAI.Infrastructure.Services;

public class EmailTemplateService : IEmailTemplateService
{
    public async Task<string> GetWaitlistConfirmationEmail(WaitlistEntry entry)
    {
        var template = await File.ReadAllTextAsync("Templates/waitlist-confirm.html");
        return template
            .Replace("{EMAIL}", entry.Email)
            .Replace("{POSITION}", entry.Position.ToString())
            .Replace("{ID}", entry.Id)
            .Replace("{ETA}", "Soon (TM)")
            .Replace("{YEAR}", DateTime.UtcNow.Year.ToString())
            .Replace("{UTC_TIME}", DateTime.UtcNow.ToString("U"));
    }

    public async Task<string> GetWaitlistInvitationEmail(WaitlistEntry entry)
    {
        var template = await File.ReadAllTextAsync("Templates/waitlist-invite.html");
        return template
            .Replace("{EMAIL}", entry.Email)
            .Replace("{EXPIRATION_TIME}", "7 days")
            .Replace("{YEAR}", DateTime.UtcNow.Year.ToString())
            .Replace("{UTC_TIME}", DateTime.UtcNow.ToString("U"));
    }

    public async Task<string> GetSignupConfirmationEmail(AccountDTO account)
    {
        var template = await File.ReadAllTextAsync("Templates/signup-confirm.html");
        return template
            .Replace("{EMAIL}", account.Email)
            .Replace("{USERNAME}", account.Username)
            .Replace("{YEAR}", DateTime.UtcNow.Year.ToString())
            .Replace("{UTC_TIME}", DateTime.UtcNow.ToString("U"));
    }

    public async Task<string> GetPasswordResetEmail(string email, string token)
    {
        throw new NotImplementedException();
    }
}