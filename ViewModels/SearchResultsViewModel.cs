using SJOB_EXE201.Models;
using System.Collections.Generic;

namespace SJOB_EXE201.ViewModels
{
    public class SearchResultsViewModel
    {
        public string Keyword { get; set; }
        public string Location { get; set; }
        public string JobType { get; set; }
        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }
        public int CategoryId { get; set; }

        public List<JobPost> Jobs { get; set; }
        public int TotalJobs { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public List<string> SuggestedKeywords { get; set; }

        public class CategoryWithCount
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int JobCount { get; set; }
        }

        public List<CategoryWithCount> PopularCategories { get; set; }
    }
}