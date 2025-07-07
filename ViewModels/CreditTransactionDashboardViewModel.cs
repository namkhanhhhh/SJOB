using SJOB_EXE201.Data;

namespace SJOB_EXE201.ViewModels
{
    public class CreditTransactionDashboardViewModel
    {
        public int MonthlyTransactionCount { get; set; }
        public decimal MonthlyTransactionAmount { get; set; }
        public int YearlyTransactionCount { get; set; }
        public decimal YearlyTransactionAmount { get; set; }
        public List<MonthlyTransactionData> MonthlyData { get; set; }
        public List<TransactionTypeData> TransactionTypeData { get; set; }
        public List<TopUserData> TopUsers { get; set; }
        public int TotalUsers { get; set; }
        public decimal TotalCredits { get; set; }
    }
}
