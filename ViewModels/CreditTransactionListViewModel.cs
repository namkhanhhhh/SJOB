using SJOB_EXE201.Models;

namespace SJOB_EXE201.ViewModels
{
    public class CreditTransactionListViewModel
    {
        public List<CreditTransaction> Transactions { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public string SearchEmail { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public string TransactionType { get; set; }
        public string SortBy { get; set; }
        public string SortOrder { get; set; }
        public List<string> TransactionTypes { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
