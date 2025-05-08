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

    public bool Status { get; set; } = true; // Thêm trạng thái người dùng: true = hoạt động, false = không hoạt động

    public string? AuthProvider { get; set; }

    public string? AuthProviderId { get; set; }

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual ICollection<Banner> Banners { get; set; } = new List<Banner>();

    public virtual ICollection<CompanyProfile> CompanyProfiles { get; set; } = new List<CompanyProfile>();

    public virtual ICollection<CreditTransaction> CreditTransactions { get; set; } = new List<CreditTransaction>();

    public virtual ICollection<JobPost> JobPosts { get; set; } = new List<JobPost>();

    public virtual ICollection<MarketingCampaign> MarketingCampaigns { get; set; } = new List<MarketingCampaign>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<ServiceOrder> ServiceOrders { get; set; } = new List<ServiceOrder>();

    public virtual ICollection<ServiceUsage> ServiceUsages { get; set; } = new List<ServiceUsage>();

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

    public virtual ICollection<UserCredit> UserCredits { get; set; } = new List<UserCredit>();

    public virtual ICollection<UserDetail> UserDetails { get; set; } = new List<UserDetail>();

    public virtual ICollection<WorkerVisit> WorkerVisits { get; set; } = new List<WorkerVisit>();

    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}
