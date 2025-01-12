using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LSC.SmartCertify.Domain.Entities;

public partial class Course
{
    [Key]
    public int CourseId { get; set; }

    [StringLength(100)]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("Courses")]
    public virtual UserProfile CreatedByNavigation { get; set; } = null!;

    [InverseProperty("Course")]
    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();

    [InverseProperty("Course")]
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
