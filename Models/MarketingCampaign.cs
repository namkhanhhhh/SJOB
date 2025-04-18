using System;
using System.Collections.Generic;

namespace SJOB_EXE201.Models;

public partial class MarketingCampaign
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? EmailTemplate { get; set; }

    public int? TargetCount { get; set; }

    public int? SentCount { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
