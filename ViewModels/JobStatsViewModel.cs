using SJOB_EXE201.Models;

namespace SJOB_EXE201.ViewModels
{
    public class JobStatsViewModel
    {
        public JobPost JobPost { get; set; }
        public int TotalApplications { get; set; }
        public int PendingApplications { get; set; }
        public int AcceptedApplications { get; set; }
        public int RejectedApplications { get; set; }
        public int TotalViews { get; set; }
        public int UniqueViewers { get; set; }
        public double ConversionRate { get; set; }
    }
}
