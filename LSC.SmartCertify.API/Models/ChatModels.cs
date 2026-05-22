namespace LSC.SmartCertify.API.Models;

/// <summary>Inbound request from Angular chat component.</summary>
public class ChatRequest
{
    /// <summary>The user's latest message (max 500 chars).</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Full conversation history so far (excluding the current message).</summary>
    public List<ConversationMessage> History { get; set; } = new();

    /// <summary>
    /// Persisted chat session ID. Null on the very first message — the server creates a new session
    /// and returns the ID. Angular must include this on every subsequent turn.
    /// </summary>
    public int? SessionId { get; set; }
}

/// <summary>A single turn in the conversation, sent back and forth.</summary>
public class ConversationMessage
{
    /// <summary>"user" or "assistant"</summary>
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

/// <summary>Response returned to Angular after each chat turn.</summary>
public class ChatResponse
{
    /// <summary>Claude's reply to display in the chat bubble.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Set when Claude has created an exam. Angular uses this to show the Start Exam button.</summary>
    public int? ExamId { get; set; }

    /// <summary>True when an exam was successfully created this turn.</summary>
    public bool ExamCreated { get; set; }

    /// <summary>Conversational history including this turn — Angular persists this for the next request.</summary>
    public List<ConversationMessage> UpdatedHistory { get; set; } = new();

    /// <summary>
    /// Chat session ID — created on the first turn, echoed back on subsequent turns.
    /// Angular must persist this and include it in every ChatRequest.
    /// </summary>
    public int SessionId { get; set; }
}

/// <summary>Result returned by AnthropicService.ProcessTurnAsync.</summary>
public class ChatTurnResult
{
    public string Message { get; set; } = string.Empty;
    public int? ExamId { get; set; }
    /// <summary>Comma-separated distinct tool names invoked this turn (e.g. "search_courses,fetch_questions").</summary>
    public string? ToolsInvoked { get; set; }
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
}
