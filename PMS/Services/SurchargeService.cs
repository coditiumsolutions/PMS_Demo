using PMS.Models;

namespace PMS.Services
{
    public class SurchargeService : ISurchargeService
    {
        public IReadOnlyDictionary<string, SurchargeComputationRow> ComputeBySchedule(
            IEnumerable<PaymentSchedule> schedules,
            string? customerId,
            DateTime asOfDate)
        {
            var result = new Dictionary<string, SurchargeComputationRow>(StringComparer.OrdinalIgnoreCase);
            var customerIdTrimmed = (customerId ?? string.Empty).Trim();

            foreach (var schedule in schedules)
            {
                if (string.IsNullOrWhiteSpace(schedule.ScheduleID))
                {
                    continue;
                }

                var payments = (schedule.Payments ?? new List<Payment>())
                    .Where(p => string.Equals(p.AuditStatus, "Approved", StringComparison.OrdinalIgnoreCase)
                        && string.Equals((p.CustomerID ?? string.Empty).Trim(), customerIdTrimmed, StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(p.Status, "Pending", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(p => p.PaymentDate)
                    .ToList();

                var amountPaid = payments.Sum(p => p.Amount);
                var outstanding = Math.Max(0m, schedule.Amount - amountPaid);
                var surcharge = 0m;
                var daysOverdue = 0;
                var dailyRatePercent = 0m;
                var dailySurchargeAmount = 0m;

                if (schedule.SurchargeApplied)
                {
                    var dueDate = schedule.DueDate.Date;
                    var endDate = asOfDate.Date;

                    if (amountPaid >= schedule.Amount && payments.Count > 0)
                    {
                        endDate = payments[^1].PaymentDate.Date;
                    }

                    if (endDate > dueDate)
                    {
                        daysOverdue = (int)(endDate - dueDate).TotalDays;
                        var isOldFormat = schedule.SurchargeRate > 1m;
                        var dailyRateDecimal = isOldFormat ? schedule.SurchargeRate / 100m : schedule.SurchargeRate;
                        dailyRatePercent = isOldFormat ? schedule.SurchargeRate : (schedule.SurchargeRate * 100m);

                        var amountForSurcharge = amountPaid >= schedule.Amount ? schedule.Amount : outstanding;
                        dailySurchargeAmount = amountForSurcharge * dailyRateDecimal;
                        surcharge = Math.Round(dailySurchargeAmount * daysOverdue, 2, MidpointRounding.AwayFromZero);
                    }
                }

                result[schedule.ScheduleID] = new SurchargeComputationRow
                {
                    ScheduleID = schedule.ScheduleID,
                    AmountPaid = amountPaid,
                    Outstanding = outstanding,
                    Surcharge = surcharge,
                    DaysOverdue = daysOverdue,
                    DailyRatePercent = dailyRatePercent,
                    DailySurchargeAmount = dailySurchargeAmount
                };
            }

            return result;
        }
    }
}
