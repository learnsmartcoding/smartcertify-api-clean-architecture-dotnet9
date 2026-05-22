using LSC.SmartCertify.Application.Interfaces.Chat;
using LSC.SmartCertify.Domain.Entities;

namespace LSC.SmartCertify.Application.Services;

public class ChatHistoryService(IChatHistoryRepository repository) : IChatHistoryService
{
    public async Task<int> StartSessionAsync(int userId, string firstUserMessage, CancellationToken ct)
    {
        // Use the first 200 chars of the user's opening message as the session title
        var title = firstUserMessage.Length > 200
            ? firstUserMessage[..200].TrimEnd() + "…"
            : firstUserMessage;

        var session = new ChatSession
        {
            UserId     = userId,
            Title      = title,
            StartedOn  = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow
        };

        var created = await repository.CreateSessionAsync(session, ct);
        return created.ChatSessionId;
    }

    public async Task AppendMessagesAsync(
        int sessionId,
        string userMessage,
        string assistantMessage,
        string? toolsInvoked,
        int inputTokens,
        int outputTokens,
        CancellationToken ct)
    {
        // Get the current message count to compute the next sequence numbers
        var existingCount = await repository.GetMessageCountAsync(sessionId, ct);

        var messages = new List<ChatMessage>
        {
            new()
            {
                ChatSessionId = sessionId,
                Role          = "user",
                Content       = userMessage,
                Sequence      = existingCount + 1,
                CreatedOn     = DateTime.UtcNow
            },
            new()
            {
                ChatSessionId = sessionId,
                Role          = "assistant",
                Content       = assistantMessage,
                Sequence      = existingCount + 2,
                ToolsInvoked  = toolsInvoked,
                InputTokens   = inputTokens > 0 ? inputTokens : null,
                OutputTokens  = outputTokens > 0 ? outputTokens : null,
                CreatedOn     = DateTime.UtcNow
            }
        };

        await repository.AddMessagesAsync(messages, ct);
    }

    public async Task LinkExamAsync(int sessionId, int examId, CancellationToken ct)
    {
        var session = await repository.GetSessionAsync(sessionId, ct);
        if (session is null) return;

        session.ExamId  = examId;
        session.EndedOn = DateTime.UtcNow;

        await repository.UpdateSessionAsync(session, ct);
    }

    public async Task EndSessionAsync(int sessionId, CancellationToken ct)
    {
        var session = await repository.GetSessionAsync(sessionId, ct);
        if (session is null || session.EndedOn.HasValue) return;

        session.EndedOn = DateTime.UtcNow;
        await repository.UpdateSessionAsync(session, ct);
    }
}
