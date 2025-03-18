namespace LudereAI.Application.Interfaces.Services;

public interface IAccountUsageService
{
    Task<bool> CanSendMessage(string accountId);
    Task<bool> CanAnalyseScreenshot(string accountId);
    Task IncrementUsage(string accountId, bool isMessage, bool isScreenshot);
    Task ResetUsage(string accountId);
    Task CleanupOldConversations();
}