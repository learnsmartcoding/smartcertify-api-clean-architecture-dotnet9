using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LSC.SmartCertify.Domain.Entities;

public partial class ContactU
{
    [Key]
    public int ContactUsId { get; set; }

    [StringLength(100)]
    public string UserName { get; set; } = null!;

    [StringLength(100)]
    public string UserEmail { get; set; } = null!;

    [StringLength(2000)]
    public string MessageDetail { get; set; } = null!;
}
