using SJOB_EXE201.Models;
using System;
using System.Collections.Generic;

namespace ShortJob.Models.ViewModels
{
    public class UserPackagesViewModel
    {
        public User User { get; set; }
        public UserPostCredit UserCredits { get; set; } // Thêm thông tin về credits
        public List<UserSubscriptionViewModel> Subscriptions { get; set; }
        public List<UserServiceViewModel> AdditionalServices { get; set; }
    }

    public class UserSubscriptionViewModel
    {
        public int Id { get; set; }
        public string PlanName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public int DaysRemaining { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
    }

    public class UserServiceViewModel
    {
        public int Id { get; set; }
        public string ServiceName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public int? DaysRemaining { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
    }
}