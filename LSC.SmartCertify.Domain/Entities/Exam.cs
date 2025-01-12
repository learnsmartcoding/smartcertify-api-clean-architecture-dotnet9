using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LSC.SmartCertify.Domain.Entities;

public partial class Exam
{
    [Key]
    public int ExamId { get; set; }

    public int CourseId { get; set; }

    public int UserId { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = null!;

    public DateTime StartedOn { get; set; }

    public DateTime? FinishedOn { get; set; }

    [StringLength(2000)]
    public string? Feedback { get; set; }

    [ForeignKey("CourseId")]
    [InverseProperty("Exams")]
    public virtual Course Course { get; set; } = null!;

    [InverseProperty("Exam")]
    public virtual ICollection<ExamQuestion> ExamQuestions { get; set; } = new List<ExamQuestion>();

    [ForeignKey("UserId")]
    [InverseProperty("Exams")]
    public virtual UserProfile User { get; set; } = null!;
}
