using System;
using System.Collections.Generic;

namespace SJOB_EXE201.Models;

public partial class UserCredit
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public decimal? Balance { get; set; }

    public DateTime? LastUpdated { get; set; }

    public virtual User User { get; set; } = null!;
}
