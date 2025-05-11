using System;
using System.Collections.Generic;

namespace SJOB_EXE201.Models;

public partial class ServiceOrder
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ServiceId { get; set; }

    // Add properties to track post credits applied from this order
    public int? SilverPostsApplied { get; set; }

    public int? GoldPostsApplied { get; set; }

    public int? DiamondPostsApplied { get; set; }

    public bool? PostCreditsApplied { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual AdditionalService Service { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}