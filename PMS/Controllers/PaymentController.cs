using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS.Data;
using PMS.Models;
using PMS.Services;
using System.Security.Claims;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace PMS.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private const string ModuleKey = "Payment";
        private readonly PMSDbContext _context;
        private readonly IModulePermissionService _modulePermission;

        public PaymentController(PMSDbContext context, IModulePermissionService modulePermission)
        {
            _context = context;
            _modulePermission = modulePermission;
        }

        private async Task<IActionResult?> EnsurePermissionAsync(string requiredLevel)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var perm = await _modulePermission.GetPermissionAsync(userId, ModuleKey);
            if (requiredLevel == "Read" && !_modulePermission.CanRead(perm))
                return RedirectToAction("AccessDenied", "Account");
            if (requiredLevel == "Edit" && !_modulePermission.CanEdit(perm))
                return RedirectToAction("AccessDenied", "Account");
            if (requiredLevel == "Admin" && !_modulePermission.CanDelete(perm))
                return RedirectToAction("AccessDenied", "Account");
            ViewBag.CanCreate = _modulePermission.CanEdit(perm);
            ViewBag.CanEdit = _modulePermission.CanEdit(perm);
            ViewBag.CanDelete = _modulePermission.CanDelete(perm);
            return null;
        }

        public async Task<IActionResult> Index()
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;
            var payments = await _context.Payments
                .Include(p => p.PaymentSchedule)
                    .ThenInclude(ps => ps.PaymentPlan)
                .ToListAsync();
            return View(payments);
        }

        // Payment Plans (Batches) Management
        public async Task<IActionResult> PaymentPlans()
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;
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
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;
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
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;
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

        /// <summary>Print-friendly payment receipt. Opens in new tab for printing.</summary>
        [HttpGet]
        public async Task<IActionResult> Receipt(string id)
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var payment = await _context.Payments
                .Include(p => p.Customer)
                .Include(p => p.PaymentSchedule)
                    .ThenInclude(ps => ps!.PaymentPlan)
                        .ThenInclude(pp => pp!.Project)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PaymentID == id);

            if (payment == null)
                return NotFound();

            return View(payment);
        }

        public async Task<IActionResult> PaymentSchedule(string planId)
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;
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

        /// <summary>
        /// AJAX: Returns customer info and due installments for the Record Payment form. Call after user enters Customer ID and clicks Search.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCustomerPaymentInfo(string customerId)
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;
            // #region agent log
            var _logPath = @"d:\PMS\PMS\PMS\.cursor\debug.log";
            void _agentLog(string hypothesisId, string message, object? data = null)
            {
                try
                {
                    var payload = new { runId = "run1", hypothesisId, location = "PaymentController.GetCustomerPaymentInfo", message, data, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() };
                    System.IO.File.AppendAllText(_logPath, JsonSerializer.Serialize(payload) + Environment.NewLine);
                }
                catch { }
            }
            // #endregion

            _agentLog("H1", "GetCustomerPaymentInfo entry", new { customerId, trimmed = customerId?.Trim() });

            if (string.IsNullOrWhiteSpace(customerId))
            {
                _agentLog("H1", "Return: empty customerId");
                return Json(new { found = false, message = "Please enter a Customer ID." });
            }

            try
            {
                var customer = await _context.Customers
                    .AsNoTracking()
                    .Where(c => c.CustomerID == customerId.Trim() && (c.Status ?? "Active") == "Active")
                    .Select(c => new { c.CustomerID, c.FullName, c.PlanID })
                    .FirstOrDefaultAsync();

                _agentLog("H1", "Customer lookup result", customer == null ? new { found = false } : new { found = true, planId = customer.PlanID });

                if (customer == null)
                {
                    _agentLog("H1", "Return: customer not found");
                    return Json(new { found = false, message = "Customer not found or inactive." });
                }

                if (string.IsNullOrWhiteSpace(customer.PlanID))
                {
                    _agentLog("H1", "Return: no plan");
                    return Json(new { found = false, message = "Customer has no payment plan assigned." });
                }

                _agentLog("H4", "Before schedules query", new { planId = customer.PlanID });

                // Load schedules without Payments nav to avoid selecting new audit columns (works when DB has no CreatedBy/CreatedAt/LastModified yet)
                var schedules = await _context.PaymentSchedules
                    .AsNoTracking()
                    .Include(ps => ps.PaymentPlan)
                    .Where(ps => ps.PlanID == customer.PlanID)
                    .OrderBy(ps => ps.DueDate)
                    .ToListAsync();

                var scheduleIds = schedules.Select(ps => ps.ScheduleID).ToList();
                var customerIdTrimmed = customerId.Trim();
                // Only sum payments made by this customer for each schedule (not all customers)
                var paidBySchedule = await _context.Payments
                    .AsNoTracking()
                    .Where(p => scheduleIds.Contains(p.ScheduleID) && p.CustomerID == customerIdTrimmed)
                    .GroupBy(p => p.ScheduleID)
                    .Select(g => new { ScheduleID = g.Key, TotalPaid = g.Sum(p => p.Amount) })
                    .ToListAsync();
                var paidLookup = paidBySchedule.ToDictionary(x => x.ScheduleID!, x => x.TotalPaid);

                var dueSchedules = schedules
                    .Select(ps =>
                    {
                        var paid = paidLookup.TryGetValue(ps.ScheduleID, out var p) ? p : 0m;
                        var outstanding = Math.Max(0m, ps.Amount - paid);
                        return new
                        {
                            scheduleId = ps.ScheduleID,
                            planName = ps.PaymentPlan?.PlanName,
                            paymentDescription = ps.PaymentDescription,
                            installmentNo = ps.InstallmentNo,
                            dueDate = ps.DueDate.ToString("MMM dd, yyyy"),
                            isOverdue = ps.DueDate.Date < DateTime.Today,
                            amount = ps.Amount,
                            paid = paid,
                            outstanding = outstanding,
                            surchargeApplied = ps.SurchargeApplied,
                            surchargeRate = ps.SurchargeRate
                        };
                    })
                    .Where(x => x.outstanding > 0)
                    .ToList();

                _agentLog("H4", "Return: found true", new { scheduleCount = dueSchedules.Count });
                return Json(new
                {
                    found = true,
                    customerId = customer.CustomerID,
                    fullName = string.IsNullOrWhiteSpace(customer.FullName) ? "(No Name)" : customer.FullName,
                    schedules = dueSchedules
                });
            }
            catch (Exception ex)
            {
                _agentLog("H1", "Exception", new { exType = ex.GetType().FullName, exMessage = ex.Message });
                throw;
            }
        }

        public async Task<IActionResult> RecordPayment(string scheduleId = null, string customerId = null)
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
            // Pre-fill from link (e.g. from Payment Schedule page): load schedule for info card and pass IDs for view to run search on load
            ViewBag.PreSelectedCustomerId = null;
            ViewBag.PreSelectedScheduleId = null;
            ViewBag.Schedule = null;
            ViewBag.ScheduleOutstanding = 0m;

            if (!string.IsNullOrWhiteSpace(scheduleId))
            {
                var preSelectedSchedule = await _context.PaymentSchedules
                    .AsNoTracking()
                    .Include(ps => ps.PaymentPlan)
                    .Include(ps => ps.Payments)
                    .FirstOrDefaultAsync(ps => ps.ScheduleID == scheduleId);

                if (preSelectedSchedule != null)
                {
                    ViewBag.Schedule = preSelectedSchedule;
                    ViewBag.PreSelectedScheduleId = scheduleId;
                    var paidAmount = preSelectedSchedule.Payments?.Sum(p => p.Amount) ?? 0m;
                    ViewBag.ScheduleOutstanding = Math.Max(0m, preSelectedSchedule.Amount - paidAmount);

                    if (!string.IsNullOrWhiteSpace(preSelectedSchedule.PlanID))
                    {
                        var preSelectedCustomerId = await _context.Customers
                            .AsNoTracking()
                            .Where(c => c.PlanID == preSelectedSchedule.PlanID && (c.Status ?? "Active") == "Active")
                            .OrderBy(c => c.CustomerID)
                            .Select(c => c.CustomerID)
                            .FirstOrDefaultAsync();
                        if (!string.IsNullOrWhiteSpace(preSelectedCustomerId))
                            ViewBag.PreSelectedCustomerId = preSelectedCustomerId;
                    }
                }
            }
            else if (!string.IsNullOrWhiteSpace(customerId))
            {
                ViewBag.PreSelectedCustomerId = customerId;
            }

            ViewBag.PaymentStatuses = new List<string> { "Pending", "Paid", "Partially Paid" };

            return View();
        }

        // Backward-compatible route alias for mistyped/deployed links.
        [HttpGet]
        public IActionResult RecordPaymentv(string scheduleId = null, string customerId = null)
        {
            return RedirectToAction(nameof(RecordPayment), new { scheduleId, customerId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordPayment(string customerId, string scheduleId, decimal amount, string method, string referenceNo, string remarks, string status = "Paid")
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
            if (string.IsNullOrWhiteSpace(customerId) || string.IsNullOrWhiteSpace(scheduleId))
            {
                TempData["Error"] = "Customer and installment are required. Search by Customer ID first and select an installment.";
                return RedirectToAction(nameof(RecordPayment));
            }

            if (amount <= 0)
            {
                TempData["Error"] = "Amount must be greater than zero.";
                return RedirectToAction(nameof(RecordPayment), new { customerId, scheduleId });
            }

            var customer = await _context.Customers
                .AsNoTracking()
                .Where(c => c.CustomerID == customerId.Trim() && (c.Status ?? "Active") == "Active")
                .Select(c => new { c.CustomerID, c.PlanID })
                .FirstOrDefaultAsync();

            if (customer == null)
            {
                TempData["Error"] = "Customer not found or inactive. Please search again.";
                return RedirectToAction(nameof(RecordPayment));
            }

            var schedule = await _context.PaymentSchedules
                .AsNoTracking()
                .Include(ps => ps.Payments)
                .FirstOrDefaultAsync(ps => ps.ScheduleID == scheduleId);

            if (schedule == null)
            {
                TempData["Error"] = "Installment not found.";
                return RedirectToAction(nameof(RecordPayment), new { customerId });
            }

            if (schedule.PlanID != customer.PlanID)
            {
                TempData["Error"] = "Selected installment does not belong to this customer's plan.";
                return RedirectToAction(nameof(RecordPayment), new { customerId });
            }

            var paidSoFar = schedule.Payments?.Where(p => p.CustomerID == customerId.Trim()).Sum(p => p.Amount) ?? 0m;
            var totalDue = Math.Max(0m, schedule.Amount - paidSoFar);
            if (amount > totalDue)
            {
                TempData["Error"] = $"Amount Received (PKR {amount:N2}) must not exceed Total Due for this installment (PKR {totalDue:N2}). You can record multiple partial payments.";
                return RedirectToAction(nameof(RecordPayment), new { customerId, scheduleId });
            }

            var isFullPayment = (amount >= totalDue);
            if (isFullPayment && string.Equals(status, "Partially Paid", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "When paying the full amount (Total Due = Total Paid), status cannot be Partially Paid. Use Paid.";
                return RedirectToAction(nameof(RecordPayment), new { customerId, scheduleId });
            }
            if (!isFullPayment && string.Equals(status, "Paid", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "When paying a partial amount (Total Due ≠ Total Paid), status cannot be Paid. Use Partially Paid.";
                return RedirectToAction(nameof(RecordPayment), new { customerId, scheduleId });
            }

            var payment = new Payment
            {
                PaymentID = GenerateID(),
                CustomerID = customerId.Trim(),
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
            var userName = User.Identity?.Name ?? User.FindFirst(ClaimTypes.Name)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                var actionText = string.IsNullOrEmpty(userName)
                    ? "Record Payment"
                    : $"Record Payment - {userName}";
                await LogActivity(userId, actionText, "Payment", payment.PaymentID);
            }

            TempData["Success"] = $"Payment of PKR {amount:N2} recorded. Outstanding for this installment: PKR {totalDue - amount:N2}. You can record another payment if needed.";
            return RedirectToAction(nameof(RecordPayment), new { customerId = customerId.Trim(), scheduleId });
        }

        // ─── Multiple Payments ────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> MultiplePayments(string? customerId = null)
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
            ViewBag.PreSelectedCustomerId = customerId;
            return View();
        }

        public class MultiPaymentRow
        {
            public string? ScheduleId { get; set; }
            public decimal Amount { get; set; }
        }

        public class MultiPaymentRequest
        {
            public string? CustomerId { get; set; }
            public decimal TotalAmount { get; set; }
            public string? ReferenceNo { get; set; }
            public string? Method { get; set; }
            public string? Remarks { get; set; }
            public DateTime PaymentDate { get; set; }
            public List<MultiPaymentRow>? Payments { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MultiplePayments([FromBody] MultiPaymentRequest request)
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;

            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.CustomerId))
                    return Json(new { success = false, message = "Customer ID is required." });

                if (request.Payments == null || !request.Payments.Any(p => p.Amount > 0))
                    return Json(new { success = false, message = "At least one payment row with amount > 0 is required." });

                var rows = request.Payments.Where(p => p.Amount > 0).ToList();

                // Validate sum equals total
                var sumRows = rows.Sum(p => p.Amount);
                if (Math.Abs(sumRows - request.TotalAmount) > 0.01m)
                    return Json(new { success = false, message = $"Sum of installment amounts (PKR {sumRows:N2}) must equal Total Payment (PKR {request.TotalAmount:N2})." });

                var customer = await _context.Customers.AsNoTracking()
                    .Where(c => c.CustomerID == request.CustomerId.Trim() && (c.Status ?? "Active") == "Active")
                    .Select(c => new { c.CustomerID, c.PlanID })
                    .FirstOrDefaultAsync();

                if (customer == null)
                    return Json(new { success = false, message = "Customer not found or inactive." });

                var scheduleIds = rows.Select(r => r.ScheduleId).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

                var schedules = await _context.PaymentSchedules.AsNoTracking()
                    .Include(ps => ps.Payments)
                    .Where(ps => scheduleIds.Contains(ps.ScheduleID) && ps.PlanID == customer.PlanID)
                    .ToListAsync();

                if (schedules.Count != scheduleIds.Count)
                    return Json(new { success = false, message = "One or more selected installments do not belong to this customer's plan." });

                var paymentDate = request.PaymentDate == default ? DateTime.Now : request.PaymentDate;
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userName = User.Identity?.Name ?? User.FindFirst(ClaimTypes.Name)?.Value;
                var savedIds = new List<string>();

                foreach (var row in rows)
                {
                    var schedule = schedules.First(s => s.ScheduleID == row.ScheduleId);
                    var paidSoFar = schedule.Payments?.Where(p => p.CustomerID == request.CustomerId.Trim()).Sum(p => p.Amount) ?? 0m;
                    var outstanding = Math.Max(0m, schedule.Amount - paidSoFar);

                    if (row.Amount > outstanding + 0.01m)
                        return Json(new { success = false, message = $"Amount for Installment #{schedule.InstallmentNo} (PKR {row.Amount:N2}) exceeds outstanding (PKR {outstanding:N2})." });

                    var isFullPayment = row.Amount >= outstanding - 0.01m;
                    var status = isFullPayment ? "Paid" : "Partially Paid";

                    var payment = new Payment
                    {
                        PaymentID   = GenerateID(),
                        CustomerID  = request.CustomerId.Trim(),
                        ScheduleID  = row.ScheduleId,
                        PaymentDate = paymentDate,
                        Amount      = row.Amount,
                        Method      = request.Method,
                        ReferenceNo = request.ReferenceNo,
                        Status      = status,
                        Remarks     = request.Remarks
                    };

                    _context.Payments.Add(payment);
                    savedIds.Add(payment.PaymentID);
                }

                await _context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(userId))
                {
                    var actionText = string.IsNullOrEmpty(userName)
                        ? $"Record Multiple Payments ({rows.Count} installments)"
                        : $"Record Multiple Payments ({rows.Count} installments) - {userName}";
                    foreach (var pid in savedIds)
                        await LogActivity(userId, actionText, "Payment", pid);
                }

                return Json(new { success = true, count = savedIds.Count, message = $"{savedIds.Count} payment(s) recorded successfully (PKR {sumRows:N2} total)." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePayment(string paymentId)
        {
            var denied = await EnsurePermissionAsync("Admin");
            if (denied != null) return denied;
            if (string.IsNullOrWhiteSpace(paymentId))
            {
                TempData["Error"] = "Payment ID is required.";
                return RedirectToAction(nameof(CustomerPayments));
            }

            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null)
            {
                TempData["Error"] = "Payment not found.";
                return RedirectToAction(nameof(CustomerPayments));
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                var actionText = $"Delete Payment. CustomerID: {payment.CustomerID ?? ""}";
                await LogActivity(userId, actionText, "Payment", payment.PaymentID);
            }

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Payment deleted successfully.";
            return RedirectToAction(nameof(CustomerPayments));
        }

        [HttpGet]
        public async Task<IActionResult> EditPayment(string paymentId)
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
            if (string.IsNullOrWhiteSpace(paymentId))
            {
                TempData["Error"] = "Payment ID is required.";
                return RedirectToAction(nameof(CustomerPayments));
            }

            var payment = await _context.Payments
                .Include(p => p.Customer)
                .Include(p => p.PaymentSchedule)
                    .ThenInclude(ps => ps.PaymentPlan)
                .FirstOrDefaultAsync(p => p.PaymentID == paymentId);

            if (payment == null)
            {
                TempData["Error"] = "Payment not found.";
                return RedirectToAction(nameof(CustomerPayments));
            }

            if (payment.PaymentSchedule != null)
            {
                var otherPaymentsSum = await _context.Payments
                    .Where(p => p.ScheduleID == payment.ScheduleID && p.PaymentID != paymentId)
                    .SumAsync(p => p.Amount);
                ViewBag.MaxAmount = payment.PaymentSchedule.Amount - otherPaymentsSum;
            }
            else
            {
                ViewBag.MaxAmount = payment.Amount;
            }

            ViewBag.PaymentStatuses = new List<string> { "Pending", "Paid", "Partially Paid" };

            return View(payment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPayment(string paymentId, DateTime paymentDate, decimal amount, string method, string referenceNo, string status, string remarks)
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
            if (string.IsNullOrWhiteSpace(paymentId))
            {
                TempData["Error"] = "Payment ID is required.";
                return RedirectToAction(nameof(CustomerPayments));
            }

            var payment = await _context.Payments
                .Include(p => p.PaymentSchedule)
                    .ThenInclude(ps => ps.PaymentPlan)
                .FirstOrDefaultAsync(p => p.PaymentID == paymentId);

            if (payment == null)
            {
                TempData["Error"] = "Payment not found.";
                return RedirectToAction(nameof(CustomerPayments));
            }

            if (amount <= 0)
            {
                TempData["Error"] = "Amount must be greater than zero.";
                return RedirectToAction(nameof(EditPayment), new { paymentId });
            }

            var schedule = payment.PaymentSchedule;
            if (schedule != null)
            {
                var otherPaymentsSum = await _context.Payments
                    .Where(p => p.ScheduleID == schedule.ScheduleID && p.PaymentID != paymentId)
                    .SumAsync(p => p.Amount);
                var maxAllowed = schedule.Amount - otherPaymentsSum;
                if (amount > maxAllowed)
                {
                    TempData["Error"] = $"Amount (PKR {amount:N2}) must not exceed the remaining due for this installment (PKR {maxAllowed:N2}).";
                    return RedirectToAction(nameof(EditPayment), new { paymentId });
                }
            }

            payment.PaymentDate = paymentDate;
            payment.Amount = amount;
            payment.Method = method;
            payment.ReferenceNo = referenceNo;
            payment.Status = status;
            payment.Remarks = remarks;

            await _context.SaveChangesAsync();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                var userName = User.Identity?.Name ?? User.FindFirst(ClaimTypes.Name)?.Value;
                var actionText = string.IsNullOrEmpty(userName) ? "Edit Payment" : $"Edit Payment - {userName}";
                await LogActivity(userId, actionText, "Payment", payment.PaymentID);
            }

            TempData["Success"] = "Payment updated successfully.";
            return RedirectToAction(nameof(CustomerPayments));
        }

        [HttpGet]
        public async Task<IActionResult> CreatePaymentPlan()
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
            ViewBag.Projects = await _context.Projects.ToListAsync();
            ViewBag.UsdToPkrRate = await GetUsdToPkrRateAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePaymentPlan([FromBody] PaymentPlanCreateViewModel viewModel)
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
            try
            {
                if (viewModel?.PaymentPlan == null || viewModel?.PaymentSchedules == null)
                {
                    return Json(new { success = false, message = "Invalid data provided" });
                }

                var planData = viewModel.PaymentPlan;
                if (string.IsNullOrWhiteSpace(planData.ProjectID))
                {
                    return Json(new { success = false, message = "Project is required." });
                }
                var scheduleData = viewModel.PaymentSchedules;

                var exchangeRate = planData.ExchangeRate > 0 ? planData.ExchangeRate : await GetUsdToPkrRateAsync();

                if (exchangeRate <= 0)
                {
                    return Json(new { success = false, message = "Invalid USD to PKR exchange rate configuration." });
                }

                if (planData.TotalAmount <= 0 && planData.TotalAmountUSD <= 0)
                {
                    return Json(new { success = false, message = "Total amount must be provided in either PKR or USD." });
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
                        message = $"Total Installments Amount (PKR {totalInstallmentsAmount:N2}) + Token Amount (PKR {tokenAmount:N2}) = PKR {grandTotal:N2} exceeds Parent Total Amount (PKR {planData.TotalAmount:N2}). Excess: PKR {excess:N2}." 
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

        private async Task<decimal> GetUsdToPkrRateAsync()
        {
            const decimal defaultRate = 1m;
            var config = await _context.Configurations
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ConfigKey == "Currency:USDToPKR");

            if (config != null && decimal.TryParse(config.ConfigValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) && parsed > 0)
            {
                return parsed;
            }

            return defaultRate;
        }

        [HttpGet]
        public async Task<IActionResult> CreatePaymentSchedule(string planId)
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
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
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
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
                ModelState.AddModelError("Amount", $"Installments total would exceed plan total. Remaining allowed: {remaining:N2} PKR.");
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
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
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
                    TempData["Error"] = $"Installments total would exceed plan total. Remaining allowed: {remaining:N2} PKR.";
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
            var denied = await EnsurePermissionAsync("Admin");
            if (denied != null) return denied;
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
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;
            var penalties = await _context.Penalties
                .Include(p => p.Customer)
                .ToListAsync();
            
            ViewBag.Customers = await _context.Customers.ToListAsync();
            return View(penalties);
        }

        [HttpPost]
        public async Task<IActionResult> AddPenalty(string customerId, decimal amount, string reason)
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
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
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;
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
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
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
