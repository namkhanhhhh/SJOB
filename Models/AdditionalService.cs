using System;
using System.Collections.Generic;

namespace SJOB_EXE201.Models;

public partial class AdditionalService
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int? DurationDays { get; set; }

    public string? ServiceType { get; set; }

    // Add properties to track post counts included in this service
    public int? SilverPostsIncluded { get; set; }

    public int? GoldPostsIncluded { get; set; }

    public int? DiamondPostsIncluded { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<ServiceOrder> ServiceOrders { get; set; } = new List<ServiceOrder>();
}