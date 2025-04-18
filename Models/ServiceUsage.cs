using System;
using System.Collections.Generic;

namespace SJOB_EXE201.Models;

public partial class ServiceUsage
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int JobPostId { get; set; }

    public string? ServiceType { get; set; }

    public int? ReferenceId { get; set; }

    public string? ReferenceType { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual JobPost JobPost { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
