using SJOB_EXE201.Models;
using System.Collections.Generic;

namespace SJOB_EXE201.ViewModels
{
    public class EmployerHistoryViewModel
    {
        public List<JobPost> JobPosts { get; set; }
        public List<ServiceOrder> ServiceOrders { get; set; }
        public List<Payment> Payments { get; set; }
    }
}
