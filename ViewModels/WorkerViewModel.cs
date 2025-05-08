using SJOB_EXE201.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SJOB_EXE201.ViewModels
{
    public class WorkerViewModel
    {
        public User User { get; set; }
        public List<JobCategory> FeaturedCategories { get; set; }
        public List<JobPost> DiamondPosts { get; set; }
        public List<JobPost> MostViewedPosts { get; set; }
        public List<JobPost> AllPosts { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public string Keyword { get; set; }
        public string Location { get; set; }
        public string JobType { get; set; }
        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }
        public int CategoryId { get; set; }
    }

}
