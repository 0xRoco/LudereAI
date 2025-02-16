using LudereAI.Application.Interfaces.Services;
using LudereAI.Domain.Models.Account;
using LudereAI.Domain.Models.Tiers;
using LudereAI.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LudereAI.Infrastructure.Services;

public class AccountUsageService : IAccountUsageService
{
    private readonly DatabaseContext _context;
    private readonly TierLimitsConfig _tierLimits;


    public AccountUsageService(DatabaseContext context, IOptions<TierLimitsConfig> tierLimits)
    {
        _context = context;
        _tierLimits = tierLimits.Value;
    }

    public async Task<bool> CanSendMessage(string accountId)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == accountId);

        if (account == null) return false;
        var tier = account.Tier;

        if (tier is SubscriptionTier.Ultimate) return true;
        
        var today = DateTime.UtcNow.Date;
        var usage = await GetOrCreateDailyUsage(accountId, today);
        var limit = tier switch
        {
            SubscriptionTier.Free => _tierLimits.Free.DailyMessageLimit,
            SubscriptionTier.Guest => _tierLimits.Guest.DailyMessageLimit,
            SubscriptionTier.Pro => _tierLimits.Pro.DailyMessageLimit,
            _ => 0
        };
        
        return usage.MessageCount < limit;
    }

    public async Task<bool> CanAnalyseScreenshot(string accountId)
    {
        var account = await _context.Accounts
            .Include(a => a.Subscription)
            .FirstOrDefaultAsync(a => a.Id == accountId);
        
        if (account == null) return false;
        var tier = account.Tier;
        
        if (tier is SubscriptionTier.Ultimate) return true;
        
        var today = DateTime.UtcNow.Date;
        var usage = await GetOrCreateDailyUsage(accountId, today);
        var limit = tier switch
        {
            SubscriptionTier.Free => _tierLimits.Free.DailyScreenshotLimit,
            SubscriptionTier.Guest => _tierLimits.Guest.DailyScreenshotLimit,
            SubscriptionTier.Pro => _tierLimits.Pro.DailyScreenshotLimit,
            _ => 0
        };
        
        return usage.ScreenshotCount < limit;
    }

    public async Task IncrementUsage(string accountId, bool isMessage, bool isScreenshot)
    {
        var today = DateTime.UtcNow.Date;
        var usage = await GetOrCreateDailyUsage(accountId, today);
        if (isMessage)
            usage.MessageCount++;
        if (isScreenshot)
            usage.ScreenshotCount++;
        _context.AccountUsages.Entry(usage).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task ResetUsage(string accountId)
    {
        var today = DateTime.UtcNow.Date;
        var usage = await GetOrCreateDailyUsage(accountId, today);
        
        usage.MessageCount = 0;
        usage.ScreenshotCount = 0;
        _context.AccountUsages.Entry(usage).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task CleanupOldConversations()
    {
        var accounts = await _context.Accounts
            .Include(a => a.Conversations)
            .ToListAsync();

        foreach (var account in accounts)
        {
            var tier = account.Tier;
            
            if (tier is SubscriptionTier.Ultimate) continue;
            var maxDays = tier switch
            {
                SubscriptionTier.Free => _tierLimits.Free.ConversationHistoryLimit,
                SubscriptionTier.Guest => _tierLimits.Guest.ConversationHistoryLimit,
                SubscriptionTier.Pro => _tierLimits.Pro.ConversationHistoryLimit,
                _ => 0
            };
            
            var cutOff = DateTime.UtcNow.Date.AddDays(-maxDays);

            if (account.Conversations == null) continue;
            
            var oldConversations = account.Conversations
                .Where(c => c.CreatedAt < cutOff)
                .ToList();

            foreach (var conversation in oldConversations)
            {
                conversation.DeletedAt = DateTime.UtcNow;
            }
                
            await _context.SaveChangesAsync();
        }
    }

    private async Task<AccountUsage> GetOrCreateDailyUsage(string accountId, DateTime date)
    {
        var usage = await _context.AccountUsages.FirstOrDefaultAsync(u=> u.AccountId == accountId && u.Date == date);

        if (usage != null) return usage;
        
        usage = new AccountUsage
        {
            AccountId = accountId,
            Date = date,
            MessageCount = 0,
            ScreenshotCount = 0,
        };
            
        await _context.AccountUsages.AddAsync(usage);
        await _context.SaveChangesAsync();

        return usage;
    }
}