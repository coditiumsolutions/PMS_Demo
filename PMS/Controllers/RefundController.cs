using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS.Data;
using PMS.Models;
using PMS.Services;
using System.Security.Claims;
using System.Text.Json;

namespace PMS.Controllers
{
    [Authorize]
    public class RefundController : Controller
    {
        private const string ModuleKey = "Refund";

        private readonly PMSDbContext _context;
        private readonly IModulePermissionService _modulePermission;

        public RefundController(PMSDbContext context, IModulePermissionService modulePermission)
        {
            _context = context;
            _modulePermission = modulePermission;
        }

        private async Task<IActionResult?> EnsurePermissionAsync(string requiredLevel)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");
            var perm = await _modulePermission.GetPermissionAsync(userId, ModuleKey);
            if (requiredLevel == "Read"  && !_modulePermission.CanRead(perm))  return RedirectToAction("AccessDenied", "Account");
            if (requiredLevel == "Edit"  && !_modulePermission.CanEdit(perm))  return RedirectToAction("AccessDenied", "Account");
            if (requiredLevel == "Admin" && !_modulePermission.CanDelete(perm)) return RedirectToAction("AccessDenied", "Account");
            ViewBag.CanCreate = _modulePermission.CanEdit(perm);
            ViewBag.CanEdit   = _modulePermission.CanEdit(perm);
            ViewBag.CanDelete = _modulePermission.CanDelete(perm);
            return null;
        }

        private void SetWorkflowStatusesViewBag()
        {
            var config = _context.Configurations.FirstOrDefault(c => c.ConfigKey == "refundworkflow");
            ViewBag.WorkflowStatuses = config?.ConfigValue != null
                ? config.ConfigValue.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList()
                : new List<string> { "Initiated", "Approved", "Declined" };
        }

        private async Task<string> GenerateRefundIdAsync()
        {
            var last = await _context.Refunds
                .OrderByDescending(r => r.RefundID)
                .Select(r => r.RefundID)
                .FirstOrDefaultAsync();
            int next = 1;
            if (!string.IsNullOrEmpty(last))
            {
                var numPart = last.TrimStart('R', 'F', 'D');
                if (int.TryParse(numPart, out int n)) next = n + 1;
            }
            return "RFD" + next.ToString("D7");
        }

        // GET: /Refund
        public async Task<IActionResult> Index(string workflowFilter = "", string customerFilter = "")
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;

            var query = _context.Refunds
                .Include(r => r.Customer)
                .Include(r => r.CreatedByUser)
                .Include(r => r.ApprovedByUser)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(workflowFilter))
                query = query.Where(r => r.WorkflowStatus == workflowFilter);
            if (!string.IsNullOrWhiteSpace(customerFilter))
                query = query.Where(r => r.CustomerID == customerFilter.Trim());

            var list = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
            SetWorkflowStatusesViewBag();
            ViewBag.WorkflowFilter  = workflowFilter;
            ViewBag.CustomerFilter  = customerFilter;
            ViewBag.InitiatedCount  = await _context.Refunds.CountAsync(r => r.WorkflowStatus == "Initiated");
            ViewBag.ApprovedCount   = await _context.Refunds.CountAsync(r => r.WorkflowStatus == "Approved");
            ViewBag.DeclinedCount   = await _context.Refunds.CountAsync(r => r.WorkflowStatus == "Declined");
            return View(list);
        }

        // GET: /Refund/Create
        public async Task<IActionResult> Create()
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
            SetWorkflowStatusesViewBag();
            return View(new Refund { WorkflowStatus = "Initiated", CreatedAt = DateTime.Now });
        }

        // AJAX POST: search customer and return their outstanding payments
        [HttpPost]
        public async Task<IActionResult> SearchCustomerForRefund(string customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                return Json(new { success = false, message = "Enter a Customer ID." });

            var customer = await _context.Customers
                .Include(c => c.PaymentPlan)
                    .ThenInclude(p => p!.Project)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CustomerID == customerId.Trim());

            if (customer == null)
                return Json(new { success = false, message = "Customer not found." });

            // Get payments that are not already linked to an approved refund
            var approvedRefundPaymentIds = await _context.Refunds
                .Where(r => r.WorkflowStatus == "Approved" && r.SelectedPaymentIDs != null)
                .Select(r => r.SelectedPaymentIDs)
                .ToListAsync();

            var alreadyRefundedIds = new HashSet<string>();
            foreach (var json in approvedRefundPaymentIds)
            {
                if (string.IsNullOrEmpty(json)) continue;
                try
                {
                    var ids = JsonSerializer.Deserialize<List<string>>(json);
                    if (ids != null) foreach (var id in ids) alreadyRefundedIds.Add(id);
                }
                catch { }
            }

            var payments = await _context.Payments
                .Include(p => p.PaymentSchedule)
                .Where(p => p.CustomerID == customer.CustomerID)
                .AsNoTracking()
                .OrderBy(p => p.PaymentDate)
                .ToListAsync();

            var paymentDtos = payments
                .Where(p => !alreadyRefundedIds.Contains(p.PaymentID))
                .Select(p => new
                {
                    paymentId   = p.PaymentID,
                    description = p.PaymentSchedule?.PaymentDescription ?? "—",
                    installmentNo = p.PaymentSchedule?.InstallmentNo,
                    amount      = p.Amount,
                    method      = p.Method ?? "—",
                    date        = p.PaymentDate.ToString("MMM dd, yyyy"),
                    referenceNo = p.ReferenceNo ?? "—",
                    status      = p.Status ?? "—"
                }).ToList();

            return Json(new
            {
                success     = true,
                customerID  = customer.CustomerID,
                fullName    = customer.FullName ?? "—",
                cnic        = customer.CNIC ?? "—",
                phone       = customer.Phone ?? "—",
                project     = customer.PaymentPlan?.Project?.ProjectName ?? "—",
                payments    = paymentDtos
            });
        }

        // POST: /Refund/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Refund model, string selectedPaymentIdsJson)
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Validate customer
            if (string.IsNullOrWhiteSpace(model.CustomerID) ||
                !await _context.Customers.AnyAsync(c => c.CustomerID == model.CustomerID))
            {
                TempData["ErrorMessage"] = "Valid Customer ID is required.";
                SetWorkflowStatusesViewBag();
                return View(model);
            }

            // Validate payment selection
            List<string>? selectedIds = null;
            if (!string.IsNullOrWhiteSpace(selectedPaymentIdsJson))
            {
                try { selectedIds = JsonSerializer.Deserialize<List<string>>(selectedPaymentIdsJson); }
                catch { }
            }
            if (selectedIds == null || selectedIds.Count == 0)
            {
                TempData["ErrorMessage"] = "Select at least one payment to refund.";
                SetWorkflowStatusesViewBag();
                return View(model);
            }

            // Auto-calculate refunded amount
            model.RefundedAmount = model.PaidAmount - model.DeductionAmount;
            if (model.RefundedAmount < 0) model.RefundedAmount = 0;

            model.RefundID = await GenerateRefundIdAsync();
            model.WorkflowStatus = "Initiated";
            model.CreatedBy = userId;
            model.CreatedAt = DateTime.Now;
            model.SelectedPaymentIDs = JsonSerializer.Serialize(selectedIds);

            _context.Refunds.Add(model);

            _context.ActivityLogs.Add(new ActivityLog
            {
                UserID    = userId,
                Action    = $"Refund {model.RefundID} initiated for customer {model.CustomerID}. Amount: PKR {model.PaidAmount:N0}",
                RefType   = "Refund",
                RefID     = model.RefundID,
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Refund {model.RefundID} created successfully.";
            return RedirectToAction(nameof(Details), new { id = model.RefundID });
        }

        // GET: /Refund/Details/{id}
        public async Task<IActionResult> Details(string id)
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;

            var refund = await _context.Refunds
                .Include(r => r.Customer)
                    .ThenInclude(c => c!.PaymentPlan)
                        .ThenInclude(p => p!.Project)
                .Include(r => r.CreatedByUser)
                .Include(r => r.ApprovedByUser)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RefundID == id);

            if (refund == null) return NotFound();

            // Resolve selected payments for display
            List<Payment>? selectedPayments = null;
            if (!string.IsNullOrEmpty(refund.SelectedPaymentIDs))
            {
                try
                {
                    var ids = JsonSerializer.Deserialize<List<string>>(refund.SelectedPaymentIDs);
                    if (ids != null && ids.Count > 0)
                    {
                        selectedPayments = await _context.Payments
                            .Include(p => p.PaymentSchedule)
                            .Where(p => ids.Contains(p.PaymentID))
                            .AsNoTracking()
                            .ToListAsync();
                    }
                }
                catch { }
            }

            ViewBag.SelectedPayments = selectedPayments ?? new List<Payment>();
            return View(refund);
        }

        // POST: /Refund/Approve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(string refundId, string? notes)
        {
            var denied = await EnsurePermissionAsync("Admin");
            if (denied != null) return denied;

            var refund = await _context.Refunds.FindAsync(refundId);
            if (refund == null)
            {
                TempData["ErrorMessage"] = "Refund not found.";
                return RedirectToAction(nameof(Index));
            }
            if (refund.WorkflowStatus != "Initiated")
            {
                TempData["ErrorMessage"] = $"Cannot approve a refund that is already {refund.WorkflowStatus}.";
                return RedirectToAction(nameof(Details), new { id = refundId });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Delete selected payments and log each deletion
            List<string>? selectedIds = null;
            if (!string.IsNullOrEmpty(refund.SelectedPaymentIDs))
            {
                try { selectedIds = JsonSerializer.Deserialize<List<string>>(refund.SelectedPaymentIDs); }
                catch { }
            }

            if (selectedIds != null && selectedIds.Count > 0)
            {
                var paymentsToDelete = await _context.Payments
                    .Where(p => selectedIds.Contains(p.PaymentID))
                    .ToListAsync();

                foreach (var payment in paymentsToDelete)
                {
                    _context.ActivityLogs.Add(new ActivityLog
                    {
                        UserID    = userId,
                        Action    = $"Payment {payment.PaymentID} (PKR {payment.Amount:N0}) deleted as part of Refund {refundId} approval.",
                        RefType   = "Payment",
                        RefID     = payment.PaymentID,
                        CreatedAt = DateTime.Now
                    });
                }
                _context.Payments.RemoveRange(paymentsToDelete);
            }

            // Update refund
            refund.WorkflowStatus = "Approved";
            refund.ApprovedBy     = userId;
            refund.ApprovedAt     = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(notes)) refund.Notes = notes;
            _context.Refunds.Update(refund);

            _context.ActivityLogs.Add(new ActivityLog
            {
                UserID    = userId,
                Action    = $"Refund {refundId} approved. {selectedIds?.Count ?? 0} payment(s) deleted.",
                RefType   = "Refund",
                RefID     = refundId,
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Refund {refundId} approved. {selectedIds?.Count ?? 0} payment(s) removed.";
            return RedirectToAction(nameof(Details), new { id = refundId });
        }

        // POST: /Refund/Decline
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Decline(string refundId, string? notes)
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;

            var refund = await _context.Refunds.FindAsync(refundId);
            if (refund == null)
            {
                TempData["ErrorMessage"] = "Refund not found.";
                return RedirectToAction(nameof(Index));
            }
            if (refund.WorkflowStatus != "Initiated")
            {
                TempData["ErrorMessage"] = $"Cannot decline a refund that is already {refund.WorkflowStatus}.";
                return RedirectToAction(nameof(Details), new { id = refundId });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            refund.WorkflowStatus = "Declined";
            refund.ApprovedBy     = userId;
            refund.ApprovedAt     = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(notes)) refund.Notes = notes;
            _context.Refunds.Update(refund);

            _context.ActivityLogs.Add(new ActivityLog
            {
                UserID    = userId,
                Action    = $"Refund {refundId} declined.",
                RefType   = "Refund",
                RefID     = refundId,
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Refund {refundId} has been declined.";
            return RedirectToAction(nameof(Details), new { id = refundId });
        }
    }
}
