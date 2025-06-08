using System;
using System.Collections.Generic;

namespace SJOB_EXE201.Models;

public partial class User
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string? Password { get; set; }

    public string Username { get; set; } = null!;

    public int RoleId { get; set; }

    public string? Avatar { get; set; }

    public bool Status { get; set; }

    public string? AuthProvider { get; set; }

    public string? AuthProviderId { get; set; }

    // Add property to track available post counts
    public virtual UserPostCredit? UserPostCredit { get; set; }

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual ICollection<CreditTransaction> CreditTransactions { get; set; } = new List<CreditTransaction>();

    public virtual ICollection<JobPost> JobPosts { get; set; } = new List<JobPost>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<ServiceOrder> ServiceOrders { get; set; } = new List<ServiceOrder>();

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

    public virtual ICollection<UserCredit> UserCredits { get; set; } = new List<UserCredit>();

    public virtual ICollection<UserDetail> UserDetails { get; set; } = new List<UserDetail>();

    public virtual ICollection<WorkerVisit> WorkerVisits { get; set; } = new List<WorkerVisit>();
}