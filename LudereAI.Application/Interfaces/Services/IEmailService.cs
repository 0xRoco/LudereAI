namespace LudereAI.Application.Interfaces.Services;

public interface IEmailService
{
    Task SendEmail(string to, string subject, string body);
}