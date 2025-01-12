using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LSC.SmartCertify.Domain.Entities;

[Table("Notification")]
public partial class Notification
{
    [Key]
    public int NotificationId { get; set; }

    [StringLength(200)]
    public string Subject { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public DateTime ScheduledSendTime { get; set; }

    public bool IsActive { get; set; }

    [InverseProperty("Notification")]
    public virtual ICollection<UserNotification> UserNotifications { get; set; } = new List<UserNotification>();
}
