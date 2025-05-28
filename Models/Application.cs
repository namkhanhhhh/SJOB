using System;
using System.Collections.Generic;

namespace SJOB_EXE201.Models;

public partial class Application
{
    public int Id { get; set; }

    public int JobPostId { get; set; }

    public int UserId { get; set; }

    public string? Status { get; set; }

    public int? EmployerRating { get; set; }

    public int? WorkerRating { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual JobPost JobPost { get; set; } = null!;

    public virtual ICollection<ApplicationNote> ApplicationNotes { get; set; } = new List<ApplicationNote>();

    public virtual User User { get; set; } = null!;
}
