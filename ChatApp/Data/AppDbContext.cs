using ChatApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ChatSession> Sessions { get; set; } = null!;
    public DbSet<ChatMessage> Messages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChatSession>()
            .HasMany(s => s.Messages)
            .WithOne(m => m.ChatSession)
            .HasForeignKey(m => m.ChatSessionId);
    }
}
