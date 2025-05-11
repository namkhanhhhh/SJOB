using SJOB_EXE201.Models;

namespace SJOB_EXE201.ViewModels
{
    public class CreditTransactionDetailsViewModel
    {
        public CreditTransaction Transaction { get; set; }
        public ServiceOrder ServiceOrder { get; set; }
        public Subscription Subscription { get; set; }

        // User activity data
        public int TotalTransactions { get; set; }
        public decimal TotalTopup { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal CreditBalance { get; set; }
        public int ActiveServices { get; set; }
        public int ActiveSubscriptions { get; set; }
        public List<CreditTransaction> RecentTransactions { get; set; }
    }
}
