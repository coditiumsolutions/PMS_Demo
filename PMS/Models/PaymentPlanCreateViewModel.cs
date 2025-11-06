namespace PMS.Models
{
    public class PaymentPlanCreateViewModel
    {
        public PaymentPlanData? PaymentPlan { get; set; }
        public PaymentScheduleData? PaymentSchedules { get; set; }
    }

    public class PaymentPlanData
    {
        public string? PlanName { get; set; }
        public string? ProjectID { get; set; }
        public decimal TotalAmount { get; set; }
        public int DurationMonths { get; set; }
        public string? Frequency { get; set; }
        public string? Description { get; set; }
    }

    public class PaymentScheduleData
    {
        public bool IncludeToken { get; set; }
        public decimal? TokenAmount { get; set; }
        public int TotalInstallments { get; set; }
        public decimal InstallmentAmount { get; set; }
        public string? Frequency { get; set; }
        public DateTime FirstInstallmentDueDate { get; set; }
        public string? PaymentDescription { get; set; }
        public bool SurchargeApplied { get; set; } = true;
        public decimal SurchargeRate { get; set; } = 0.05m;
    }
}

