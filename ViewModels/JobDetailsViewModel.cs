using SJOB_EXE201.Models;

namespace SJOB_EXE201.ViewModels
{
    public class JobDetailsViewModel
    {
        public JobPost JobPost { get; set; }
        public List<JobPost> RelatedJobs { get; set; }
        public bool HasApplied { get; set; }
        public bool IsInWishlist { get; set; } // Add this property

    }
}
