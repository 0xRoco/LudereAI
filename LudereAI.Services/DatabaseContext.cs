using LudereAI.Core.Entities.Chat;
using LudereAI.Core.Entities.Media;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LudereAI.Services;

public sealed class DatabaseContext : DbContext
{
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Screenshot> Screenshots => Set<Screenshot>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    
    public DatabaseContext(DbContextOptions<DatabaseContext> options, ILogger<DatabaseContext> logger) : base(options)
    {
    }
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Conversation>()
            .HasMany(e => e.Messages)
            .WithOne(m => m.Conversation)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Screenshot)
            .WithOne(s => s.Message)
            .HasForeignKey<Screenshot>(s => s.MessageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}