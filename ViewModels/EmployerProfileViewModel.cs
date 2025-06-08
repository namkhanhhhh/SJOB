// EmployerProfileViewModel.cs
using SJOB_EXE201.Models;
using System;
using System.Collections.Generic;

namespace SJOB_EXE201.ViewModels
{
    public class EmployerProfileViewModel
    {
        public User Employer { get; set; }
        public UserDetail UserDetail { get; set; }
        public int? JobPostsCount { get; set; }
        public int? ApplicationsCount { get; set; }
        public int? ViewsCount { get; set; }
        public int RemainingPostsCount { get; set; }  // Tổng số lượt đăng còn lại
        public int TotalJobPosts { get; set; }
        public List<JobPost> JobPosts { get; set; }
        public int ProfileViewCount { get; set; }
        public DateTime JoinedDate { get; set; }
        public List<int> WishlistJobIds { get; set; }
    }
}