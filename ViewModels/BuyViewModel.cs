using SJOB_EXE201.Models;

namespace SJOB_EXE201.ViewModels
{
    public class BuyViewModel
    {
        public string Type { get; set; }
        public int Id { get; set; }
        public AdditionalService? Service { get; set; }
        public SubscriptionPlan? Combo { get; set; }
    }

}
