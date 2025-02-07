namespace LudereAI.Application.Interfaces.Services;

public interface IAccountUsageService
{
    Task<bool> CanSendMessage(string accountId);
    Task<bool> CanAnalyseScreenshot(string accountId);
    Task IncrementMessageCount(string accountId);
    Task IncrementScreenshotCount(string accountId);
    Task ResetUsage(string accountId);
    Task CleanupOldConversations();
}