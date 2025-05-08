using SJOB_EXE201.Models;
using System.Collections.Generic;

namespace SJOB_EXE201.ViewModels
{
    public class WishlistViewModel
    {
        public List<JobPost> WishlistJobs { get; set; }
        public int TotalJobs { get; set; }
    }
}