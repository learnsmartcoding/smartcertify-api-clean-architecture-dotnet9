using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LSC.SmartCertify.Domain.Entities;

[Table("SmartApp")]
public partial class SmartApp
{
    [Key]
    public int SmartAppId { get; set; }

    [StringLength(50)]
    public string AppName { get; set; } = null!;

    [InverseProperty("SmartApp")]
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
