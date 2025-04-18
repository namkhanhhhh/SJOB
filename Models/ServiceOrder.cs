using System;
using System.Collections.Generic;

namespace SJOB_EXE201.Models;

public partial class ServiceOrder
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int ServiceId { get; set; }

    public int? JobPostId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual AdditionalService Service { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
