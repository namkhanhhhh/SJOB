using SJOB_EXE201.Models;
using System.Collections.Generic;

namespace SJOB_EXE201.ViewModels
{
    public class ApplicationsViewModel
    {
        public List<JobPost> JobPosts { get; set; }
        public List<Application> Applications { get; set; }
        public int? SelectedJobId { get; set; }
        public string SelectedStatus { get; set; }
        public string SearchTerm { get; set; }
        public int PendingCount { get; set; }
        public int AcceptedCount { get; set; }
        public int RejectedCount { get; set; }
        public int TotalCount { get; set; }
    }
}

