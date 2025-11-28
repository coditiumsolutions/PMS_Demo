using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS.Data;
using PMS.Models;
using System.Security.Claims;
using System.Globalization;
using System.Linq;

namespace PMS.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly PMSDbContext _context;

        public PaymentController(PMSDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var payments = await _context.Payments
                .Include(p => p.PaymentSchedule)
                    .ThenInclude(ps => ps.PaymentPlan)
                .ToListAsync();
            return View(payments);
        }

        // Payment Plans (Batches) Management
        public async Task<IActionResult> PaymentPlans()
        {
            var paymentPlans = await _context.PaymentPlans
                .Include(pp => pp.Project)
                .Include(pp => pp.Customers)
                .Include(pp => pp.PaymentSchedules)
                .ToListAsync();
            return View(paymentPlans);
        }

        // Payment Schedules (Installments) Management
        public async Task<IActionResult> Schedules()
        {
            var schedules = await _context.PaymentSchedules
                .Include(ps => ps.PaymentPlan)
                    .ThenInclude(pp => pp.Customers)
                .Include(ps => ps.Payments)
                .OrderBy(ps => ps.DueDate)
                .ToListAsync();
            return View(schedules);
        }

        // Customer Payments - All payments received from customers  
        public async Task<IActionResult> CustomerPayments(string customerId = null)
        {
            // If customerId is provided, show specific customer
            if (!string.IsNullOrEmpty(customerId))
            {
                var customer = await _context.Customers
                    .Include(c => c.PaymentPlan)
                        .ThenInclude(pp => pp.PaymentSchedules)
                            .ThenInclude(ps => ps.Payments)
                    .FirstOrDefaultAsync(c => c.CustomerID == customerId);

                if (customer == null)
                {
                    return NotFound();
                }

                ViewBag.SingleCustomer = true;
                return View("CustomerPaymentDetails", customer);
            }

            // Otherwise, show all customer payments
            var payments = await _context.Payments
                .Include(p => p.Customer)
                .Include(p => p.PaymentSchedule)
                    .ThenInclude(ps => ps.PaymentPlan)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
            
            return View(payments);
        }

        public async Task<IActionResult> PaymentSchedule(string planId)
        {
            if (string.IsNullOrEmpty(planId))
            {
                return NotFound();
            }

            var paymentPlan = await _context.PaymentPlans
                .Include(pp => pp.PaymentSchedules)
                .Include(pp => pp.Customers)
                .FirstOrDefaultAsync(pp => pp.PlanID == planId);

            if (paymentPlan == null)
            {
                return NotFound();
            }

            return View(paymentPlan);
        }

        public async Task<IActionResult> RecordPayment(string scheduleId = null, string customerId = null)
        {
            var customers = await _context.Customers
                .Where(c => c.Status == "Active")
                .OrderBy(c => c.FullName)
                .ToListAsync();

            ViewBag.Customers = customers;

            var allSchedules = await _context.PaymentSchedules
                .Include(ps => ps.PaymentPlan)
                    .ThenInclude(pp => pp.Customers)
                .Include(ps => ps.Payments)
                .OrderBy(ps => ps.DueDate)
                .ToListAsync();

            PaymentSchedule? preSelectedSchedule = null;

            if (!string.IsNullOrEmpty(scheduleId))
            {
                preSelectedSchedule = allSchedules.FirstOrDefault(ps => ps.ScheduleID == scheduleId);

                if (preSelectedSchedule == null)
                {
                    preSelectedSchedule = await _context.PaymentSchedules
                        .Include(ps => ps.PaymentPlan)
                            .ThenInclude(pp => pp.Customers)
                        .Include(ps => ps.Payments)
                        .FirstOrDefaultAsync(ps => ps.ScheduleID == scheduleId);

                    if (preSelectedSchedule != null)
                    {
                        allSchedules.Add(preSelectedSchedule);
                    }
                }

                if (preSelectedSchedule != null)
                {
                    ViewBag.Schedule = preSelectedSchedule;
                    ViewBag.PreSelectedScheduleId = scheduleId;

                    var scheduleCustomer = preSelectedSchedule.PaymentPlan?.Customers?.FirstOrDefault();
                    if (scheduleCustomer != null)
                    {
                        ViewBag.PreSelectedCustomerId = scheduleCustomer.CustomerID;
                    }

                    var paidAmount = preSelectedSchedule.Payments?.Sum(p => p.Amount) ?? 0m;
                    ViewBag.ScheduleOutstanding = Math.Max(0m, preSelectedSchedule.Amount - paidAmount);
                }
            }
            else if (!string.IsNullOrEmpty(customerId))
            {
                ViewBag.PreSelectedCustomerId = customerId;
            }

            var unpaidSchedules = allSchedules
                .Where(ps =>
                {
                    var paid = ps.Payments?.Sum(p => p.Amount) ?? 0m;
                    return ps.Amount > paid;
                })
                .ToList();

            if (preSelectedSchedule != null && !unpaidSchedules.Any(ps => ps.ScheduleID == preSelectedSchedule.ScheduleID))
            {
                unpaidSchedules.Add(preSelectedSchedule);
            }

            ViewBag.PaymentSchedules = unpaidSchedules
                .OrderBy(ps => ps.DueDate)
                .ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordPayment(string customerId, string scheduleId, decimal amount, string method, string referenceNo, string remarks, string status = "Completed")
        {
            if (string.IsNullOrEmpty(customerId) || string.IsNullOrEmpty(scheduleId) || amount <= 0)
            {
                TempData["Error"] = "Please fill all required fields.";
                return RedirectToAction(nameof(RecordPayment));
            }

            var payment = new Payment
            {
                PaymentID = GenerateID(),
                CustomerID = customerId,
                ScheduleID = scheduleId,
                PaymentDate = DateTime.Now,
                Amount = amount,
                Method = method,
                ReferenceNo = referenceNo,
                Status = status,
                Remarks = remarks
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await LogActivity(userId, "Record Payment", "Payment", payment.PaymentID);
            }

            TempData["Success"] = "Payment recorded successfully!";
            return RedirectToAction(nameof(CustomerPayments));
        }

        [HttpGet]
        public async Task<IActionResult> CreatePaymentPlan()
        {
            ViewBag.Projects = await _context.Projects.ToListAsync();
            ViewBag.UsdToSspRate = await GetUsdToSspRateAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePaymentPlan([FromBody] PaymentPlanCreateViewModel viewModel)
        {
            try
            {
                if (viewModel?.PaymentPlan == null || viewModel?.PaymentSchedules == null)
                {
                    return Json(new { success = false, message = "Invalid data provided" });
                }

                var planData = viewModel.PaymentPlan;
                var scheduleData = viewModel.PaymentSchedules;

                var exchangeRate = planData.ExchangeRate > 0 ? planData.ExchangeRate : await GetUsdToSspRateAsync();

                if (exchangeRate <= 0)
                {
                    return Json(new { success = false, message = "Invalid USD to SSP exchange rate configuration." });
                }

                if (planData.TotalAmount <= 0 && planData.TotalAmountUSD <= 0)
                {
                    return Json(new { success = false, message = "Total amount must be provided in either SSP or USD." });
                }

                if (planData.TotalAmount <= 0)
                {
                    planData.TotalAmount = Math.Round(planData.TotalAmountUSD * exchangeRate, 2, MidpointRounding.AwayFromZero);
                }
                if (planData.TotalAmountUSD <= 0)
                {
                    planData.TotalAmountUSD = Math.Round(planData.TotalAmount / exchangeRate, 2, MidpointRounding.AwayFromZero);
                }

                // Validate total amount: Token + Installments must not exceed Plan Total Amount
                var totalInstallments = scheduleData.TotalInstallments;
                var totalPlanAmount = planData.TotalAmount;
                var tokenAmount = scheduleData.IncludeToken && scheduleData.TokenAmount.HasValue ? scheduleData.TokenAmount.Value : 0;
                var tokenAmountUSD = scheduleData.IncludeToken && scheduleData.TokenAmountUSD.HasValue
                    ? scheduleData.TokenAmountUSD.Value
                    : (scheduleData.IncludeToken ? Math.Round(tokenAmount / exchangeRate, 2, MidpointRounding.AwayFromZero) : 0);

                if (totalInstallments <= 0)
                {
                    return Json(new { success = false, message = "Total installments must be greater than zero." });
                }

                var distributableAmount = totalPlanAmount - tokenAmount;
                if (distributableAmount < 0)
                {
                    return Json(new { success = false, message = "Token amount cannot exceed total plan amount." });
                }

                var baseInstallmentAmount = Math.Round(distributableAmount / totalInstallments, 2, MidpointRounding.AwayFromZero);
                var totalBaseAmount = baseInstallmentAmount * Math.Max(totalInstallments - 1, 0);
                var lastInstallmentAmount = Math.Round(distributableAmount - totalBaseAmount, 2, MidpointRounding.AwayFromZero);
                var baseInstallmentAmountUSD = Math.Round(baseInstallmentAmount / exchangeRate, 2, MidpointRounding.AwayFromZero);
                var lastInstallmentAmountUSD = Math.Round(lastInstallmentAmount / exchangeRate, 2, MidpointRounding.AwayFromZero);

                if (lastInstallmentAmount < 0)
                {
                    return Json(new { success = false, message = "Calculated installment amount is negative. Please review the input values." });
                }

                var totalInstallmentsAmount = totalBaseAmount + lastInstallmentAmount;
                decimal grandTotal = totalInstallmentsAmount + tokenAmount;

                if (grandTotal > planData.TotalAmount)
                {
                    decimal excess = grandTotal - planData.TotalAmount;
                    return Json(new { 
                        success = false, 
                        message = $"Total Installments Amount (SSP {totalInstallmentsAmount:N2}) + Token Amount (SSP {tokenAmount:N2}) = SSP {grandTotal:N2} exceeds Parent Total Amount (SSP {planData.TotalAmount:N2}). Excess: SSP {excess:N2}." 
                    });
                }

                // Create PaymentPlan
                var paymentPlan = new PaymentPlan
                {
                    PlanID = GenerateID(),
                    PlanName = planData.PlanName,
                    ProjectID = string.IsNullOrEmpty(planData.ProjectID) ? null : planData.ProjectID,
                    TotalAmount = planData.TotalAmount,
                    TotalAmountUSD = planData.TotalAmountUSD,
                    ExchangeRate = exchangeRate,
                    Currency = planData.Currency,
                    DurationMonths = planData.DurationMonths,
                    Frequency = planData.Frequency,
                    Description = planData.Description,
                    CreatedAt = DateTime.Now
                };

                _context.PaymentPlans.Add(paymentPlan);
                await _context.SaveChangesAsync();

                // Create PaymentSchedules
                var schedules = new List<PaymentSchedule>();
                var includeToken = scheduleData.IncludeToken;
                var frequency = scheduleData.Frequency ?? "Monthly";
                var firstDueDate = scheduleData.FirstInstallmentDueDate;
                var paymentDescription = scheduleData.PaymentDescription ?? "Installment";
                var surchargeApplied = scheduleData.SurchargeApplied;
                var surchargeRate = scheduleData.SurchargeRate;

                int installmentNo = 0;

                // Precompute installment amounts
                var installmentAmounts = new List<decimal>();
                var installmentAmountsUSD = new List<decimal>();
                for (int i = 0; i < totalInstallments; i++)
                {
                    if (i == totalInstallments - 1)
                    {
                        installmentAmounts.Add(lastInstallmentAmount);
                        installmentAmountsUSD.Add(lastInstallmentAmountUSD);
                    }
                    else
                    {
                        installmentAmounts.Add(baseInstallmentAmount);
                        installmentAmountsUSD.Add(baseInstallmentAmountUSD);
                    }
                }

                // Create Token if included (Installment 0 - no surcharge)
                if (includeToken && scheduleData.TokenAmount.HasValue)
                {
                    var tokenSchedule = new PaymentSchedule
                    {
                        ScheduleID = GenerateID(),
                        PlanID = paymentPlan.PlanID,
                        PaymentDescription = paymentDescription,
                        InstallmentNo = 0,
                        DueDate = firstDueDate,
                        Amount = scheduleData.TokenAmount.Value,
                        AmountUSD = tokenAmountUSD,
                        SurchargeApplied = false, // Token has no surcharge
                        SurchargeRate = 0m // Token has no surcharge rate
                    };
                    schedules.Add(tokenSchedule);
                    installmentNo = 1;
                }

                // Create regular installments
                for (int i = 0; i < totalInstallments; i++)
                {
                    var dueDate = CalculateDueDate(firstDueDate, frequency, i);
                    
                    var schedule = new PaymentSchedule
                    {
                        ScheduleID = GenerateID(),
                        PlanID = paymentPlan.PlanID,
                        PaymentDescription = paymentDescription,
                        InstallmentNo = installmentNo,
                        DueDate = dueDate,
                        Amount = installmentAmounts[i],
                        AmountUSD = installmentAmountsUSD[i],
                        SurchargeApplied = surchargeApplied,
                        SurchargeRate = surchargeRate
                    };
                    schedules.Add(schedule);
                    installmentNo++;
                }

                _context.PaymentSchedules.AddRange(schedules);
                await _context.SaveChangesAsync();

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    await LogActivity(userId, "Create Payment Plan", "PaymentPlan", paymentPlan.PlanID);
                }

                return Json(new { success = true, planId = paymentPlan.PlanID });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private DateTime CalculateDueDate(DateTime firstDueDate, string frequency, int installmentIndex)
        {
            return frequency switch
            {
                "Monthly" => firstDueDate.AddMonths(installmentIndex),
                "Quarterly" => firstDueDate.AddMonths(installmentIndex * 3),
                "Half Yearly" => firstDueDate.AddMonths(installmentIndex * 6),
                "Yearly" => firstDueDate.AddYears(installmentIndex),
                _ => firstDueDate.AddMonths(installmentIndex)
            };
        }

        private async Task<decimal> GetUsdToSspRateAsync()
        {
            const decimal defaultRate = 1m;
            var config = await _context.Configurations
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ConfigKey == "Currency:USDToSSP");

            if (config != null && decimal.TryParse(config.ConfigValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) && parsed > 0)
            {
                return parsed;
            }

            return defaultRate;
        }

        [HttpGet]
        public async Task<IActionResult> CreatePaymentSchedule(string planId)
        {
            var paymentPlan = await _context.PaymentPlans.FindAsync(planId);
            if (paymentPlan == null)
            {
                return NotFound();
            }

            ViewBag.PaymentPlan = paymentPlan;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePaymentSchedule(PaymentSchedule schedule)
        {
            // Convert surcharge rate from percentage to decimal
            // Form always sends percentage values, so always divide by 100
            schedule.SurchargeRate = schedule.SurchargeRate / 100m;

            // Server-side guard: total of installments must not exceed plan total
            var plan = await _context.PaymentPlans
                .Include(p => p.PaymentSchedules)
                .FirstOrDefaultAsync(p => p.PlanID == schedule.PlanID);

            if (plan == null)
            {
                return NotFound();
            }

            var existingTotal = plan.PaymentSchedules.Sum(ps => ps.Amount);
            var projectedTotal = existingTotal + schedule.Amount;
            if (projectedTotal > plan.TotalAmount)
            {
                var remaining = plan.TotalAmount - existingTotal;
                ModelState.AddModelError("Amount", $"Installments total would exceed plan total. Remaining allowed: {remaining:N2} SSP.");
            }

            var amountUsd = schedule.AmountUSD.GetValueOrDefault();
            var exchangeRate = plan.ExchangeRate.GetValueOrDefault();

            if (amountUsd <= 0 && exchangeRate > 0)
            {
                amountUsd = Math.Round(schedule.Amount / exchangeRate, 2, MidpointRounding.AwayFromZero);
                schedule.AmountUSD = amountUsd;
            }
            else if (amountUsd > 0 && schedule.Amount <= 0 && exchangeRate > 0)
            {
                schedule.Amount = Math.Round(amountUsd * exchangeRate, 2, MidpointRounding.AwayFromZero);
            }

            if (!ModelState.IsValid)
            {
                ViewBag.PaymentPlan = plan;
                return View(schedule);
            }

            schedule.ScheduleID = GenerateID();
            _context.PaymentSchedules.Add(schedule);
            await _context.SaveChangesAsync();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await LogActivity(userId, "Create Payment Schedule", "PaymentSchedule", schedule.ScheduleID);
            }

            TempData["Success"] = "Installment created successfully.";
            return RedirectToAction(nameof(PaymentSchedule), new { planId = schedule.PlanID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPaymentSchedule(PaymentSchedule schedule)
        {
            try
            {
                // Convert surcharge rate from percentage to decimal
                // Form always sends percentage values, so always divide by 100
                schedule.SurchargeRate = schedule.SurchargeRate / 100m;

                // Guard: editing installment should not cause total to exceed plan total
                var plan = await _context.PaymentPlans
                    .Include(p => p.PaymentSchedules)
                    .FirstOrDefaultAsync(p => p.PlanID == schedule.PlanID);
                if (plan == null)
                {
                    return NotFound();
                }

                // Find existing schedule to get old amount
                var existingSchedule = await _context.PaymentSchedules.AsNoTracking()
                    .FirstOrDefaultAsync(ps => ps.ScheduleID == schedule.ScheduleID);
                if (existingSchedule == null)
                {
                    return NotFound();
                }

                var amountUsd = schedule.AmountUSD.GetValueOrDefault();
                var exchangeRate = plan.ExchangeRate.GetValueOrDefault();

                if (amountUsd <= 0 && exchangeRate > 0)
                {
                    amountUsd = Math.Round(schedule.Amount / exchangeRate, 2, MidpointRounding.AwayFromZero);
                    schedule.AmountUSD = amountUsd;
                }
                else if (amountUsd > 0 && schedule.Amount <= 0 && exchangeRate > 0)
                {
                    schedule.Amount = Math.Round(amountUsd * exchangeRate, 2, MidpointRounding.AwayFromZero);
                }

                var totalWithoutThis = plan.PaymentSchedules.Where(ps => ps.ScheduleID != schedule.ScheduleID).Sum(ps => ps.Amount);
                var projectedTotal = totalWithoutThis + schedule.Amount;
                if (projectedTotal > plan.TotalAmount)
                {
                    var remaining = plan.TotalAmount - totalWithoutThis;
                    TempData["Error"] = $"Installments total would exceed plan total. Remaining allowed: {remaining:N2} SSP.";
                    return RedirectToAction(nameof(PaymentSchedule), new { planId = schedule.PlanID });
                }

                // Update existing schedule properties
                existingSchedule = await _context.PaymentSchedules.FindAsync(schedule.ScheduleID);
                if (existingSchedule == null)
                {
                    return NotFound();
                }

                existingSchedule.InstallmentNo = schedule.InstallmentNo;
                existingSchedule.PaymentDescription = schedule.PaymentDescription;
                existingSchedule.DueDate = schedule.DueDate;
                existingSchedule.Amount = schedule.Amount;
                existingSchedule.AmountUSD = schedule.AmountUSD;
                existingSchedule.SurchargeRate = schedule.SurchargeRate;
                existingSchedule.SurchargeApplied = schedule.SurchargeApplied;
                existingSchedule.Description = schedule.Description;

                await _context.SaveChangesAsync();

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    await LogActivity(userId, "Update Payment Schedule", "PaymentSchedule", schedule.ScheduleID);
                }

                TempData["Success"] = "Installment updated successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while updating the installment: {ex.Message}";
            }

            return RedirectToAction(nameof(PaymentSchedule), new { planId = schedule.PlanID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePaymentSchedule(string scheduleId, string planId)
        {
            var schedule = await _context.PaymentSchedules.FindAsync(scheduleId);
            if (schedule != null)
            {
                _context.PaymentSchedules.Remove(schedule);
                await _context.SaveChangesAsync();

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    await LogActivity(userId, "Delete Payment Schedule", "PaymentSchedule", scheduleId);
                }

                TempData["Success"] = "Installment deleted successfully.";
            }

            return RedirectToAction(nameof(PaymentSchedule), new { planId });
        }

        [HttpGet]
        public async Task<IActionResult> Penalties()
        {
            var penalties = await _context.Penalties
                .Include(p => p.Customer)
                .ToListAsync();
            
            ViewBag.Customers = await _context.Customers.ToListAsync();
            return View(penalties);
        }

        [HttpPost]
        public async Task<IActionResult> AddPenalty(string customerId, decimal amount, string reason)
        {
            if (string.IsNullOrEmpty(customerId) || amount <= 0)
            {
                return BadRequest();
            }

            var penalty = new Penalty
            {
                PenaltyID = GenerateID(),
                CustomerID = customerId,
                Amount = amount,
                Reason = reason,
                AppliedOn = DateTime.Now
            };

            _context.Penalties.Add(penalty);
            await _context.SaveChangesAsync();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await LogActivity(userId, "Add Penalty", "Penalty", penalty.PenaltyID);
            }

            return RedirectToAction(nameof(Penalties));
        }

        [HttpGet]
        public async Task<IActionResult> Waivers()
        {
            var waivers = await _context.Waivers
                .Include(w => w.Customer)
                .Include(w => w.ApprovedByUser)
                .ToListAsync();
            
            ViewBag.Customers = await _context.Customers.ToListAsync();
            return View(waivers);
        }

        [HttpPost]
        public async Task<IActionResult> AddWaiver(string customerId, decimal amount, string reason)
        {
            if (string.IsNullOrEmpty(customerId) || amount <= 0)
            {
                return BadRequest();
            }

            var waiver = new Waiver
            {
                WaiverID = GenerateID(),
                CustomerID = customerId,
                Amount = amount,
                Reason = reason,
                ApprovedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                CreatedAt = DateTime.Now
            };

            _context.Waivers.Add(waiver);
            await _context.SaveChangesAsync();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await LogActivity(userId, "Add Waiver", "Waiver", waiver.WaiverID);
            }

            return RedirectToAction(nameof(Waivers));
        }

        private async Task LogActivity(string userId, string action, string refType, string refId)
        {
            var activityLog = new ActivityLog
            {
                UserID = userId,
                Action = action,
                RefType = refType,
                RefID = refId,
                CreatedAt = DateTime.Now
            };

            _context.ActivityLogs.Add(activityLog);
            await _context.SaveChangesAsync();
        }

        private string GenerateID()
        {
            return Guid.NewGuid().ToString("N")[..10].ToUpper();
        }
    }
}
