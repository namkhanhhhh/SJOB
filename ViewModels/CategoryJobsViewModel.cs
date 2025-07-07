using SJOB_EXE201.Models;

namespace SJOB_EXE201.ViewModels
{
    public class CategoryJobsViewModel
    {
        public JobCategory Category { get; set; }
        public List<JobPost> Jobs { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
