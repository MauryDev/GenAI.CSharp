using ChatApp.Data;
using ChatApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Repositories;

public class ChatRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public ChatRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<ChatSession> CreateSessionAsync(string title = "Novo Chat")
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var session = new ChatSession { Title = title };
        context.Sessions.Add(session);
        await context.SaveChangesAsync();
        return session;
    }

    public async Task<List<ChatSession>> GetAllSessionsAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Sessions
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<ChatSession?> GetSessionWithMessagesAsync(Guid sessionId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var session = await context.Sessions
            .Include(s => s.Messages)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session != null)
        {
            session.Messages = session.Messages.OrderBy(m => m.Timestamp).ToList();
        }

        return session;
    }

    public async Task AddMessageAsync(Guid sessionId, string role, string content)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var message = new ChatMessage
        {
            ChatSessionId = sessionId,
            Role = role,
            Content = content,
            Timestamp = DateTime.UtcNow
        };
        context.Messages.Add(message);
        await context.SaveChangesAsync();
    }

    public async Task UpdateSessionTitleAsync(Guid sessionId, string title)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var session = await context.Sessions.FindAsync(sessionId);
        if (session != null)
        {
            session.Title = title;
            await context.SaveChangesAsync();
        }
    }

    public async Task DeleteSessionAsync(Guid sessionId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var session = await context.Sessions.FindAsync(sessionId);
        if (session != null)
        {
            context.Sessions.Remove(session);
            await context.SaveChangesAsync();
        }
    }
}
