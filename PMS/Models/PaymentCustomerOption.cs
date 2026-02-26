namespace PMS.Models
{
    public class PaymentCustomerOption
    {
        public string CustomerID { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PlanID { get; set; }
    }
}

