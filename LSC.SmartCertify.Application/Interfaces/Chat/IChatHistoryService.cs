namespace LSC.SmartCertify.Application.Interfaces.Chat;

public interface IChatHistoryService
{
    /// <summary>
    /// Creates a new chat session for the user. Call on the very first message.
    /// Returns the new ChatSessionId.
    /// </summary>
    Task<int> StartSessionAsync(int userId, string firstUserMessage, CancellationToken ct);

    /// <summary>
    /// Appends one user turn and one assistant turn to an existing session.
    /// </summary>
    Task AppendMessagesAsync(
        int sessionId,
        string userMessage,
        string assistantMessage,
        string? toolsInvoked,
        int inputTokens,
        int outputTokens,
        CancellationToken ct);

    /// <summary>
    /// Links a created exam to the session and marks EndedOn.
    /// </summary>
    Task LinkExamAsync(int sessionId, int examId, CancellationToken ct);

    /// <summary>
    /// Marks the session as ended (user navigated away).
    /// </summary>
    Task EndSessionAsync(int sessionId, CancellationToken ct);
}
