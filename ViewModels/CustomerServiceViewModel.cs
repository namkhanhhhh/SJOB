using SJOB_EXE201.Models;

namespace SJOB_EXE201.ViewModels
{
    public class CustomerServiceViewModel
    {
        public List<AdditionalService> AdditionalServices { get; set; }
        public List<SubscriptionPlan> SubscriptionPlans { get; set; }
    }
}
