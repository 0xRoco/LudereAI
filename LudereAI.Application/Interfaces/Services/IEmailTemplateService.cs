using LudereAI.Domain.Models;
using LudereAI.Shared.DTOs;

namespace LudereAI.Application.Interfaces.Services;

public interface IEmailTemplateService
{
    Task<string> GetWaitlistConfirmationEmail(WaitlistEntry entry);
    Task<string> GetWaitlistInvitationEmail(WaitlistEntry entry);
    Task<string> GetSignupConfirmationEmail(AccountDTO account);
    Task<string> GetPasswordResetEmail(string email, string token);
}