using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS.Data;
using PMS.Models;
using PMS.Services;

namespace PMS.Controllers
{
    [Authorize]
    public class NDCController : Controller
    {
        private const string ModuleKey = "NDC";
        private readonly PMSDbContext _context;
        private readonly IModulePermissionService _modulePermission;

        public NDCController(PMSDbContext context, IModulePermissionService modulePermission)
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

        private async Task<string[]> GetNDCWorkflowStatusesAsync()
        {
            var config = await _context.Configurations.FirstOrDefaultAsync(c => c.ConfigKey == "NDCWorkFlowStatus");
            if (config?.ConfigValue != null)
            {
                var list = config.ConfigValue.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToArray();
                if (list.Length > 0) return list;
            }
            return new[] { "Initiated", "Approved", "Declined" };
        }

        private async Task<int> GetNDCExpiryDaysAsync()
        {
            var config = await _context.Configurations.FirstOrDefaultAsync(c => c.ConfigKey == "NDCExpiry");
            if (config?.ConfigValue != null && int.TryParse(config.ConfigValue.Trim(), out var days) && days > 0)
                return days;
            return 14;
        }

        private async Task<int> GetNDCStartNormalDaysAsync()
        {
            var config = await _context.Configurations.FirstOrDefaultAsync(c => c.ConfigKey == "NDCStartNormal");
            if (config?.ConfigValue != null && int.TryParse(config.ConfigValue.Trim(), out var days) && days >= 0)
                return days;
            return 3;
        }

        private async Task<int> GetNDCStartUrgentDaysAsync()
        {
            var config = await _context.Configurations.FirstOrDefaultAsync(c => c.ConfigKey == "NDCStartUrgent");
            if (config?.ConfigValue != null && int.TryParse(config.ConfigValue.Trim(), out var days) && days >= 0)
                return days;
            return 0;
        }

        private async Task<string[]> GetNDCTypesAsync()
        {
            var config = await _context.Configurations.FirstOrDefaultAsync(c => c.ConfigKey == "NDCType");
            if (config?.ConfigValue != null)
            {
                var list = config.ConfigValue.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToArray();
                if (list.Length > 0) return list;
            }
            return new[] { "Normal Transfer", "Urgent Transfer", "Family Transfer", "Death Transfer" };
        }

        private async Task SetNDCConfigViewBagAsync()
        {
            ViewBag.WorkflowStatuses = await GetNDCWorkflowStatusesAsync();
            ViewBag.NDCTypes = await GetNDCTypesAsync();
            ViewBag.NDCExpiryDays = await GetNDCExpiryDaysAsync();
            ViewBag.NDCStartNormalDays = await GetNDCStartNormalDaysAsync();
            ViewBag.NDCStartUrgentDays = await GetNDCStartUrgentDaysAsync();
        }

        public async Task<IActionResult> Index(string customerIdFilter = "", string workflowFilter = "")
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;
            var query = _context.NDCs
                .Include(n => n.Customer)
                    .ThenInclude(c => c!.PaymentPlan)
                        .ThenInclude(p => p!.Project)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(customerIdFilter))
                query = query.Where(n => n.CustomerID != null && n.CustomerID.Contains(customerIdFilter));

            if (!string.IsNullOrWhiteSpace(workflowFilter))
                query = query.Where(n => n.WorkFlowStatus == workflowFilter);

            var list = await query.OrderByDescending(n => n.CreatedAt).ToListAsync();

            ViewBag.CustomerIdFilter = customerIdFilter ?? "";
            ViewBag.WorkflowFilter = workflowFilter ?? "";
            ViewBag.WorkflowStatuses = await GetNDCWorkflowStatusesAsync();
            return View(list);
        }

        public async Task<IActionResult> Create(string? customerId = null)
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
            var workflowStatuses = await GetNDCWorkflowStatusesAsync();
            var ndcTypes = await GetNDCTypesAsync();
            var defaultStatus = workflowStatuses.Length > 0 ? workflowStatuses[0] : "Initiated";

            ViewBag.WorkflowStatuses = workflowStatuses;
            ViewBag.NDCTypes = ndcTypes;
            ViewBag.NDCExpiryDays = await GetNDCExpiryDaysAsync();
            ViewBag.NDCStartNormalDays = await GetNDCStartNormalDaysAsync();
            ViewBag.NDCStartUrgentDays = await GetNDCStartUrgentDaysAsync();

            var model = new NDC
            {
                WorkFlowStatus = defaultStatus,
                CreatedAt = DateTime.Now
            };
            if (!string.IsNullOrWhiteSpace(customerId))
                model.CustomerID = customerId.Trim();
            return View(model);
        }

        /// <summary>AJAX: Returns customer info and payment summary (totalDue, totalPaid). Only allow NDC creation when due == paid.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetCustomerForNDC(string customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                return Json(new { success = false, message = "Customer ID is required." });

            var customer = await _context.Customers
                .Include(c => c.PaymentPlan)
                    .ThenInclude(pp => pp!.PaymentSchedules)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CustomerID == customerId.Trim());

            if (customer == null)
                return Json(new { success = false, message = "Customer not found." });

            var planId = customer.PlanID;
            if (string.IsNullOrEmpty(planId))
                return Json(new { success = false, message = "Customer has no payment plan." });

            var asOfDate = DateTime.Today;

            // Total due: sum of PaymentSchedule.Amount for schedules in customer's plan with DueDate <= asOfDate
            var totalDueAmount = await _context.PaymentSchedules
                .Where(ps => ps.PlanID == planId && ps.DueDate <= asOfDate)
                .SumAsync(ps => ps.Amount);

            // Total paid: sum of Payments for this customer
            var totalPaidAmount = await _context.Payments
                .Where(p => p.CustomerID == customerId)
                .SumAsync(p => p.Amount);

            var allPaymentClear = totalDueAmount == totalPaidAmount && totalDueAmount >= 0;

            var today = DateTime.Today;
            var hasActiveNDC = await _context.NDCs
                .AnyAsync(n => n.CustomerID == customerId.Trim() && n.NDCExpiryDate.HasValue && n.NDCExpiryDate.Value >= today);

            var message = !allPaymentClear ? "NDC can only be created when all dues are cleared (Total Due = Total Paid)."
                : hasActiveNDC ? "This customer already has an active NDC (expiry on or after today). A new NDC cannot be created until the current one has expired."
                : null;

            return Json(new
            {
                success = true,
                customerID = customer.CustomerID,
                fullName = customer.FullName,
                fatherName = customer.FatherName,
                cnic = customer.CNIC,
                phone = customer.Phone,
                email = customer.Email,
                address = customer.MailingAddress ?? customer.PermanentAddress,
                planName = customer.PaymentPlan?.PlanName,
                projectName = customer.PaymentPlan?.Project?.ProjectName,
                totalDueAmount,
                totalPaidAmount,
                allPaymentClear,
                hasActiveNDC,
                message
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NDC model)
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
            if (string.IsNullOrWhiteSpace(model.CustomerID))
            {
                await SetNDCConfigViewBagAsync();
                ModelState.AddModelError("CustomerID", "Customer ID is required.");
                return View(model);
            }

            var customer = await _context.Customers
                .Include(c => c.PaymentPlan)
                .FirstOrDefaultAsync(c => c.CustomerID == model.CustomerID.Trim());

            if (customer == null)
            {
                await SetNDCConfigViewBagAsync();
                ModelState.AddModelError("CustomerID", "Customer not found.");
                return View(model);
            }

            var planId = customer.PlanID;
            if (string.IsNullOrEmpty(planId))
            {
                await SetNDCConfigViewBagAsync();
                ModelState.AddModelError("CustomerID", "Customer has no payment plan.");
                return View(model);
            }

            model.CreatedAt = DateTime.Now;
            var asOfDate = model.CreatedAt;

            var totalDueAmount = await _context.PaymentSchedules
                .Where(ps => ps.PlanID == planId && ps.DueDate <= asOfDate)
                .SumAsync(ps => ps.Amount);

            var totalPaidAmount = await _context.Payments
                .Where(p => p.CustomerID == model.CustomerID)
                .SumAsync(p => p.Amount);

            var allPaymentClear = totalDueAmount == totalPaidAmount && totalDueAmount >= 0;
            if (!allPaymentClear)
            {
                await SetNDCConfigViewBagAsync();
                ModelState.AddModelError("", "NDC can only be created when all dues are cleared. Total Due: " + totalDueAmount.ToString("N0") + ", Total Paid: " + totalPaidAmount.ToString("N0") + ".");
                return View(model);
            }

            var today = DateTime.Today;
            var hasActiveNDC = await _context.NDCs
                .AnyAsync(n => n.CustomerID == model.CustomerID.Trim() && n.NDCExpiryDate.HasValue && n.NDCExpiryDate.Value >= today);
            if (hasActiveNDC)
            {
                await SetNDCConfigViewBagAsync();
                ModelState.AddModelError("", "This customer already has an active NDC (expiry on or after today). A new NDC cannot be created until the current one has expired.");
                return View(model);
            }

            var expiryDays = await GetNDCExpiryDaysAsync();
            var startNormalDays = await GetNDCStartNormalDaysAsync();
            var startUrgentDays = await GetNDCStartUrgentDaysAsync();
            var isUrgentType = !string.IsNullOrWhiteSpace(model.NDCType) && model.NDCType.Contains("Urgent", StringComparison.OrdinalIgnoreCase);
            var issuedDateOffsetDays = isUrgentType ? startUrgentDays : startNormalDays;
            var workflowStatuses = await GetNDCWorkflowStatusesAsync();
            var defaultStatus = workflowStatuses.Length > 0 ? workflowStatuses[0] : "Initiated";

            model.NDCID = GenerateID();
            model.CreatedAt = DateTime.Now;
            model.CreatedBy = User.Identity?.Name ?? User.FindFirst(ClaimTypes.Name)?.Value;
            model.IssuedDate = model.CreatedAt.AddDays(issuedDateOffsetDays);
            model.NDCExpiryDate = model.CreatedAt.Date.AddDays(expiryDays);
            model.TotalDueAmount = totalDueAmount;
            model.TotalDueInstallments = totalPaidAmount;
            model.AllPaymentClear = true;
            model.WorkFlowStatus ??= defaultStatus;

            _context.NDCs.Add(model);
            await _context.SaveChangesAsync();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await LogActivity(userId, "Create NDC", "NDC", model.NDCID);
            }

            TempData["Success"] = "NDC created successfully. Issued Date: " + model.IssuedDate.ToString("MMM dd, yyyy") + ", Expiry: " + model.NDCExpiryDate?.ToString("MMM dd, yyyy") + ".";
            return RedirectToAction(nameof(Details), new { id = model.NDCID });
        }

        public async Task<IActionResult> Details(string id)
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var ndc = await _context.NDCs
                .Include(n => n.Customer)
                    .ThenInclude(c => c!.PaymentPlan)
                        .ThenInclude(p => p!.Project)
                .FirstOrDefaultAsync(n => n.NDCID == id);

            if (ndc == null)
                return NotFound();

            return View(ndc);
        }

        /// <summary>Print-friendly NDC view; opens in new tab. Same theme as Account Statement.</summary>
        public async Task<IActionResult> Print(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var ndc = await _context.NDCs
                .Include(n => n.Customer)
                    .ThenInclude(c => c!.PaymentPlan)
                        .ThenInclude(p => p!.Project)
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.NDCID == id);

            if (ndc == null)
                return NotFound();

            return View(ndc);
        }

        private async Task LogActivity(string userId, string action, string refType, string refId)
        {
            try
            {
                _context.ActivityLogs.Add(new ActivityLog
                {
                    UserID = userId,
                    Action = action,
                    RefType = refType,
                    RefID = refId,
                    CreatedAt = DateTime.Now
                });
                await _context.SaveChangesAsync();
            }
            catch { /* ignore */ }
        }

        private static string GenerateID()
        {
            return Guid.NewGuid().ToString("N")[..10].ToUpper();
        }
    }
}
