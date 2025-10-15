using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS.Data;
using PMS.Models;
using System.Security.Claims;

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
            // Load customers and schedules for dropdowns
            ViewBag.Customers = await _context.Customers
                .Where(c => c.Status == "Active")
                .OrderBy(c => c.FullName)
                .ToListAsync();

            ViewBag.PaymentSchedules = await _context.PaymentSchedules
                .Include(ps => ps.PaymentPlan)
                .OrderBy(ps => ps.DueDate)
                .ToListAsync();

            // If scheduleId is provided, pre-select it
            if (!string.IsNullOrEmpty(scheduleId))
            {
                var schedule = await _context.PaymentSchedules
                    .Include(ps => ps.PaymentPlan)
                        .ThenInclude(pp => pp.Customers)
                    .FirstOrDefaultAsync(ps => ps.ScheduleID == scheduleId);

                if (schedule != null)
                {
                    ViewBag.Schedule = schedule;
                    ViewBag.PreSelectedScheduleId = scheduleId;
                    
                    // Get customer from the payment plan
                    var customer = schedule.PaymentPlan?.Customers?.FirstOrDefault();
                    if (customer != null)
                    {
                        ViewBag.PreSelectedCustomerId = customer.CustomerID;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(customerId))
            {
                ViewBag.PreSelectedCustomerId = customerId;
            }

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
        public IActionResult CreatePaymentPlan()
        {
            ViewBag.Projects = _context.Projects.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePaymentPlan(PaymentPlan paymentPlan)
        {
            if (ModelState.IsValid)
            {
                paymentPlan.PlanID = GenerateID();
                paymentPlan.CreatedAt = DateTime.Now;

                _context.PaymentPlans.Add(paymentPlan);
                await _context.SaveChangesAsync();

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    await LogActivity(userId, "Create Payment Plan", "PaymentPlan", paymentPlan.PlanID);
                }

                return RedirectToAction(nameof(PaymentSchedule), new { planId = paymentPlan.PlanID });
            }

            ViewBag.Projects = _context.Projects.ToList();
            return View(paymentPlan);
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
            // Normalize surcharge rate (allow user to input percent)
            if (schedule.SurchargeRate > 1)
            {
                schedule.SurchargeRate = schedule.SurchargeRate / 100m;
            }

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
                if (schedule.SurchargeRate > 1)
                {
                    schedule.SurchargeRate = schedule.SurchargeRate / 100;
                }

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

                var totalWithoutThis = plan.PaymentSchedules.Where(ps => ps.ScheduleID != schedule.ScheduleID).Sum(ps => ps.Amount);
                var projectedTotal = totalWithoutThis + schedule.Amount;
                if (projectedTotal > plan.TotalAmount)
                {
                    var remaining = plan.TotalAmount - totalWithoutThis;
                    TempData["Error"] = $"Installments total would exceed plan total. Remaining allowed: {remaining:N2} SSP.";
                    return RedirectToAction(nameof(PaymentSchedule), new { planId = schedule.PlanID });
                }

                _context.Update(schedule);
                await _context.SaveChangesAsync();

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    await LogActivity(userId, "Update Payment Schedule", "PaymentSchedule", schedule.ScheduleID);
                }

                TempData["Success"] = "Installment updated successfully.";
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while updating the installment.";
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
