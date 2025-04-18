using System;
using System.Collections.Generic;

namespace SJOB_EXE201.Models;

public partial class Banner
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public string? RedirectUrl { get; set; }

    public string? Position { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Status { get; set; }

    public decimal? BidAmount { get; set; }

    public int? ImpressionCount { get; set; }

    public int? ClickCount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
