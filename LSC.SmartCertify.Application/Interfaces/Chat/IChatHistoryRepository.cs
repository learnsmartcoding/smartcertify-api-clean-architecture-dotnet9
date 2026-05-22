using LSC.SmartCertify.Domain.Entities;

namespace LSC.SmartCertify.Application.Interfaces.Chat;

public interface IChatHistoryRepository
{
    Task<ChatSession> CreateSessionAsync(ChatSession session, CancellationToken ct);
    Task<ChatSession?> GetSessionAsync(int sessionId, CancellationToken ct);
    Task UpdateSessionAsync(ChatSession session, CancellationToken ct);
    Task<int> GetMessageCountAsync(int sessionId, CancellationToken ct);
    Task AddMessagesAsync(IEnumerable<ChatMessage> messages, CancellationToken ct);
}
