using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LSC.SmartCertify.Domain.Entities;

[Table("UserProfile")]
public partial class UserProfile
{
    [Key]
    public int UserId { get; set; }

    [StringLength(100)]
    public string DisplayName { get; set; } = null!;

    [StringLength(50)]
    public string FirstName { get; set; } = null!;

    [StringLength(50)]
    public string LastName { get; set; } = null!;

    [StringLength(100)]
    public string Email { get; set; } = null!;

    [StringLength(128)]
    public string AdObjId { get; set; } = null!;

    [StringLength(500)]
    public string? ProfileImageUrl { get; set; }

    public DateTime CreatedOn { get; set; }

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    [InverseProperty("User")]
    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();

    [InverseProperty("User")]
    public virtual ICollection<UserActivityLog> UserActivityLogs { get; set; } = new List<UserActivityLog>();

    [InverseProperty("User")]
    public virtual ICollection<UserNotification> UserNotifications { get; set; } = new List<UserNotification>();

    [InverseProperty("User")]
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
