using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LSC.SmartCertify.Domain.Entities;

public class ChatSession
{
    [Key]
    public int ChatSessionId { get; set; }

    public int UserId { get; set; }

    [StringLength(300)]
    public string? Title { get; set; }

    public int? ExamId { get; set; }

    public DateTime StartedOn { get; set; }

    public DateTime? EndedOn { get; set; }

    public DateTime CreatedDate { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("ChatSessions")]
    public virtual UserProfile User { get; set; } = null!;

    [ForeignKey("ExamId")]
    public virtual Exam? Exam { get; set; }

    [InverseProperty("ChatSession")]
    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
}
