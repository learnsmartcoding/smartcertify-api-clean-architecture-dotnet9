using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LSC.SmartCertify.Domain.Entities;

public class ChatMessage
{
    [Key]
    public int ChatMessageId { get; set; }

    public int ChatSessionId { get; set; }

    /// <summary>"user" or "assistant"</summary>
    [StringLength(20)]
    public string Role { get; set; } = null!;

    public string Content { get; set; } = null!;

    /// <summary>Order of this message within the session (1-based).</summary>
    public int Sequence { get; set; }

    /// <summary>Comma-separated MCP tool names invoked by the assistant in this turn (assistant messages only).</summary>
    [StringLength(500)]
    public string? ToolsInvoked { get; set; }

    /// <summary>Claude API input token count (assistant messages only).</summary>
    public int? InputTokens { get; set; }

    /// <summary>Claude API output token count (assistant messages only).</summary>
    public int? OutputTokens { get; set; }

    public DateTime CreatedOn { get; set; }

    [ForeignKey("ChatSessionId")]
    [InverseProperty("ChatMessages")]
    public virtual ChatSession ChatSession { get; set; } = null!;
}
