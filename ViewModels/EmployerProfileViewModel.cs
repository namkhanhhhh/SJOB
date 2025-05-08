// EmployerProfileViewModel.cs
using SJOB_EXE201.Models;
using System;
using System.Collections.Generic;

namespace SJOB_EXE201.ViewModels
{
    public class EmployerProfileViewModel
    {
        public User Employer { get; set; }
        public CompanyProfile CompanyProfile { get; set; }
        public int TotalJobPosts { get; set; }
        public List<JobPost> JobPosts { get; set; }
        public int ProfileViewCount { get; set; }
        public DateTime JoinedDate { get; set; }
        public List<int> WishlistJobIds { get; set; }
    }
}