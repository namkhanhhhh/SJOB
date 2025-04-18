using System;
using System.Collections.Generic;

namespace SJOB_EXE201.Models;

public partial class CompanyProfile
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? CompanyName { get; set; }

    public string? CompanyDescription { get; set; }

    public string? CompanyLogo { get; set; }

    public string? CompanyWebsite { get; set; }

    public string? CompanySize { get; set; }

    public string? Industry { get; set; }

    public bool? VerifiedBadge { get; set; }

    public int? FreePostsRemaining { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
