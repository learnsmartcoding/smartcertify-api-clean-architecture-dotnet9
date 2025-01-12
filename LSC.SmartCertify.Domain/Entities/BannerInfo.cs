using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LSC.SmartCertify.Domain.Entities;

[Table("BannerInfo")]
public partial class BannerInfo
{
    [Key]
    public int BannerId { get; set; }

    [StringLength(100)]
    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; }

    public DateTime DisplayFrom { get; set; }

    public DateTime DisplayTo { get; set; }

    public DateTime CreatedOn { get; set; }
}
