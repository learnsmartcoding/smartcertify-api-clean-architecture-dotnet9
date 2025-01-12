using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LSC.SmartCertify.Domain.Entities;

[Table("UserActivityLog")]
public partial class UserActivityLog
{
    [Key]
    public int LogId { get; set; }

    public int? UserId { get; set; }

    [StringLength(50)]
    public string ActivityType { get; set; } = null!;

    public string? ActivityDescription { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime LogDate { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("UserActivityLogs")]
    public virtual UserProfile? User { get; set; }
}
