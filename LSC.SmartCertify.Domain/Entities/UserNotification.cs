using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LSC.SmartCertify.Domain.Entities;

public partial class UserNotification
{
    [Key]
    public int UserNotificationId { get; set; }

    public int NotificationId { get; set; }

    public int UserId { get; set; }

    [StringLength(200)]
    public string EmailSubject { get; set; } = null!;

    public string EmailContent { get; set; } = null!;

    public bool NotificationSent { get; set; }

    public DateTime? SentOn { get; set; }

    public DateTime CreatedOn { get; set; }

    [ForeignKey("NotificationId")]
    [InverseProperty("UserNotifications")]
    public virtual Notification Notification { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("UserNotifications")]
    public virtual UserProfile User { get; set; } = null!;
}
