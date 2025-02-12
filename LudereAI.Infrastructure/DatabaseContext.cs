using LudereAI.Domain.Models;
using LudereAI.Domain.Models.Account;
using LudereAI.Domain.Models.Chat;
using LudereAI.Domain.Models.Media;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LudereAI.Infrastructure;

public sealed class DatabaseContext : DbContext
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<GuestAccount> Guests => Set<GuestAccount>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Screenshot> Screenshots => Set<Screenshot>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<UserSubscription> Subscriptions => Set<UserSubscription>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AccountUsage> AccountUsages => Set<AccountUsage>();
    
    
    public DatabaseContext(DbContextOptions<DatabaseContext> options, ILogger<DatabaseContext> logger) : base(options)
    {
    }
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>()
            .HasMany(e => e.Conversations)
            .WithOne(c => c.Account)
            .HasForeignKey(c => c.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Account>()
            .HasOne(e => e.Subscription)
            .WithOne()
            .HasForeignKey<UserSubscription>(s => s.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Conversation>()
            .HasMany(e => e.Messages)
            .WithOne(m => m.Conversation)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Screenshot)
            .WithOne(s => s.Message)
            .HasForeignKey<Screenshot>(m => m.MessageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}