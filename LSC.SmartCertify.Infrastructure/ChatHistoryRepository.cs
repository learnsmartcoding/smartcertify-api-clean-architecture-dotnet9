using LSC.SmartCertify.Application.Interfaces.Chat;
using LSC.SmartCertify.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LSC.SmartCertify.Infrastructure;

public class ChatHistoryRepository(SmartCertifyContext db) : IChatHistoryRepository
{
    public async Task<ChatSession> CreateSessionAsync(ChatSession session, CancellationToken ct)
    {
        await db.ChatSessions.AddAsync(session, ct);
        await db.SaveChangesAsync(ct);
        return session;
    }

    public async Task<ChatSession?> GetSessionAsync(int sessionId, CancellationToken ct) =>
        await db.ChatSessions.FindAsync([sessionId], ct);

    public async Task UpdateSessionAsync(ChatSession session, CancellationToken ct)
    {
        db.ChatSessions.Update(session);
        await db.SaveChangesAsync(ct);
    }

    public async Task<int> GetMessageCountAsync(int sessionId, CancellationToken ct) =>
        await db.ChatMessages.CountAsync(m => m.ChatSessionId == sessionId, ct);

    public async Task AddMessagesAsync(IEnumerable<ChatMessage> messages, CancellationToken ct)
    {
        await db.ChatMessages.AddRangeAsync(messages, ct);
        await db.SaveChangesAsync(ct);
    }
}
