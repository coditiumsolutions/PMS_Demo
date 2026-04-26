using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS.Data;
using PMS.Models;
using PMS.Services;
using System.Security.Claims;

namespace PMS.Controllers
{
    [Authorize]
    public class WaiverController : Controller
    {
        private const string ModuleKey = "Waiver";
        private const string WorkflowConfigKey = "WaiverWorkFlow";
        private const string SurchargeWaivedOffAccountHead = "Surcharge Waived Off";
        private readonly PMSDbContext _context;
        private readonly IModulePermissionService _modulePermission;
        private readonly ISurchargeService _surchargeService;

        public WaiverController(
            PMSDbContext context,
            IModulePermissionService modulePermission,
            ISurchargeService surchargeService)
        {
            _context = context;
            _modulePermission = modulePermission;
            _surchargeService = surchargeService;
        }

        private async Task<IActionResult?> EnsurePermissionAsync(string requiredLevel)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");
            var perm = await _modulePermission.GetPermissionAsync(userId, ModuleKey);
            if (requiredLevel == "Read" && !_modulePermission.CanRead(perm)) return RedirectToAction("AccessDenied", "Account");
            if (requiredLevel == "Edit" && !_modulePermission.CanEdit(perm)) return RedirectToAction("AccessDenied", "Account");
            if (requiredLevel == "Admin" && !_modulePermission.CanDelete(perm)) return RedirectToAction("AccessDenied", "Account");
            ViewBag.CanCreate = _modulePermission.CanEdit(perm);
            ViewBag.CanEdit = _modulePermission.CanEdit(perm);
            ViewBag.CanDelete = _modulePermission.CanDelete(perm);
            return null;
        }

        private async Task<List<string>> GetWorkflowStatusesAsync()
        {
            var config = await _context.Configurations.FirstOrDefaultAsync(c => c.ConfigKey == WorkflowConfigKey);
            var parsed = config?.ConfigValue?
                .Split(',')
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
            return parsed is { Count: > 0 } ? parsed : new List<string> { "Initiated", "Approved", "Declined" };
        }

        private static string ResolveWorkflowStatus(IEnumerable<string> statuses, params string[] aliases)
        {
            foreach (var alias in aliases)
            {
                var matched = statuses.FirstOrDefault(s => string.Equals(s, alias, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(matched))
                {
                    return matched;
                }
            }
            return aliases.FirstOrDefault() ?? string.Empty;
        }

        private static int GetWorkflowIndex(List<string> statuses, string? currentStatus)
        {
            if (statuses.Count == 0 || string.IsNullOrWhiteSpace(currentStatus)) return -1;
            for (var i = 0; i < statuses.Count; i++)
            {
                if (string.Equals(statuses[i], currentStatus, StringComparison.OrdinalIgnoreCase))
                    return i;
            }
            return -1;
        }

        private async Task SetWorkflowViewBagAsync()
        {
            var statuses = await GetWorkflowStatusesAsync();
            ViewBag.WorkflowStatuses = statuses;
            ViewBag.InitiatedStatus = ResolveWorkflowStatus(statuses, "Initiated", "Initialted", statuses[0]);
            ViewBag.ApprovedStatus = ResolveWorkflowStatus(statuses, "Approved");
            ViewBag.DeclinedStatus = ResolveWorkflowStatus(statuses, "Declined");
        }

        public async Task<IActionResult> Index(string statusFilter = "", string customerFilter = "")
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;

            var query = _context.Waivers
                .Include(w => w.Customer)
                .Include(w => w.CreatedByUser)
                .Include(w => w.ApprovedByUser)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(statusFilter))
                query = query.Where(w => w.Status == statusFilter);
            if (!string.IsNullOrWhiteSpace(customerFilter))
                query = query.Where(w => w.CustomerID == customerFilter.Trim());

            var list = await query.OrderByDescending(w => w.CreatedAt).ToListAsync();
            await SetWorkflowViewBagAsync();

            var workflowStatuses = ViewBag.WorkflowStatuses as List<string> ?? new List<string> { "Initiated", "Approved", "Declined" };
            var initiatedStatus = ResolveWorkflowStatus(workflowStatuses, "Initiated", "Initialted", workflowStatuses[0]);
            var approvedStatus = ResolveWorkflowStatus(workflowStatuses, "Approved");
            var declinedStatus = ResolveWorkflowStatus(workflowStatuses, "Declined");

            ViewBag.StatusFilter = statusFilter;
            ViewBag.CustomerFilter = customerFilter;
            ViewBag.InitiatedCount = await _context.Waivers.CountAsync(x => x.Status == initiatedStatus);
            ViewBag.ApprovedCount = await _context.Waivers.CountAsync(x => x.Status == approvedStatus);
            ViewBag.DeclinedCount = await _context.Waivers.CountAsync(x => x.Status == declinedStatus);
            return View(list);
        }

        public async Task<IActionResult> Create(string? customerId = null)
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
            await SetWorkflowViewBagAsync();

            var statuses = ViewBag.WorkflowStatuses as List<string> ?? new List<string> { "Initiated", "Approved", "Declined" };
            var initiatedStatus = ResolveWorkflowStatus(statuses, "Initiated", "Initialted", statuses[0]);
            ViewBag.PreSelectedCustomerId = customerId?.Trim();
            return View(new Waiver
            {
                WaiverType = "Surcharge Waiver",
                AccountHead = SurchargeWaivedOffAccountHead,
                Status = initiatedStatus
            });
        }

        [HttpPost]
        public async Task<IActionResult> SearchCustomerForWaiver(string customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                return Json(new { success = false, message = "Enter a Customer ID." });

            var customer = await _context.Customers
                .Include(c => c.PaymentPlan)
                    .ThenInclude(p => p!.Project)
                .Include(c => c.PaymentPlan)
                    .ThenInclude(p => p!.PaymentSchedules)
                        .ThenInclude(ps => ps.Payments)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CustomerID == customerId.Trim());

            if (customer == null)
                return Json(new { success = false, message = "Customer not found." });

            var schedules = customer.PaymentPlan?.PaymentSchedules?.OrderBy(x => x.InstallmentNo ?? int.MaxValue).ToList()
                ?? new List<PaymentSchedule>();

            var surchargeMap = _surchargeService.ComputeBySchedule(schedules, customer.CustomerID, DateTime.Now.Date);
            var lines = schedules.Select(s =>
            {
                surchargeMap.TryGetValue(s.ScheduleID, out var row);
                return new
                {
                    scheduleId = s.ScheduleID,
                    installmentNo = s.InstallmentNo,
                    dueDate = s.DueDate.ToString("MMM dd, yyyy"),
                    amount = s.Amount,
                    paid = row?.AmountPaid ?? 0m,
                    outstanding = row?.Outstanding ?? s.Amount,
                    surcharge = row?.Surcharge ?? 0m
                };
            }).ToList();

            var totalSurcharge = surchargeMap.Values.Sum(x => x.Surcharge);
            return Json(new
            {
                success = true,
                customerID = customer.CustomerID,
                fullName = customer.FullName ?? "—",
                cnic = customer.CNIC ?? "—",
                phone = customer.Phone ?? "—",
                project = customer.PaymentPlan?.Project?.ProjectName ?? "—",
                plan = customer.PaymentPlan?.PlanName ?? "—",
                lines,
                totalSurcharge
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Waiver model)
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
            await SetWorkflowViewBagAsync();

            model.CustomerID = model.CustomerID?.Trim();
            if (string.IsNullOrWhiteSpace(model.CustomerID))
                ModelState.AddModelError(nameof(model.CustomerID), "Customer ID is required.");

            var customer = await _context.Customers
                .Include(c => c.PaymentPlan)
                    .ThenInclude(p => p!.PaymentSchedules)
                        .ThenInclude(ps => ps.Payments)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CustomerID == model.CustomerID);

            if (customer == null)
                ModelState.AddModelError(nameof(model.CustomerID), "Customer not found.");

            decimal totalSurcharge = 0m;
            if (customer != null)
            {
                var schedules = customer.PaymentPlan?.PaymentSchedules ?? new List<PaymentSchedule>();
                var surchargeMap = _surchargeService.ComputeBySchedule(schedules, customer.CustomerID, DateTime.Now.Date);
                totalSurcharge = surchargeMap.Values.Sum(x => x.Surcharge);
            }

            model.TotalAmount = Math.Round(totalSurcharge, 2, MidpointRounding.AwayFromZero);

            if (model.WaivedAmount <= 0m && (!model.WaivedPercentage.HasValue || model.WaivedPercentage.Value <= 0m))
                ModelState.AddModelError(nameof(model.WaivedAmount), "Enter Waived Amount or Waived Percentage.");

            if (model.WaivedPercentage.HasValue && model.WaivedPercentage.Value > 100m)
                ModelState.AddModelError(nameof(model.WaivedPercentage), "Waived Percentage must be between 0 and 100.");

            if (model.WaivedAmount <= 0m && model.WaivedPercentage.HasValue && model.WaivedPercentage.Value > 0m)
            {
                model.WaivedAmount = Math.Round(model.TotalAmount * (model.WaivedPercentage.Value / 100m), 2, MidpointRounding.AwayFromZero);
            }
            else if (model.WaivedAmount > 0m && model.TotalAmount > 0m)
            {
                model.WaivedPercentage = Math.Round((model.WaivedAmount / model.TotalAmount) * 100m, 2, MidpointRounding.AwayFromZero);
            }

            if (model.WaivedAmount <= 0m)
                ModelState.AddModelError(nameof(model.WaivedAmount), "Waived Amount must be greater than zero.");
            if (model.TotalAmount > 0m && model.WaivedAmount > model.TotalAmount)
                ModelState.AddModelError(nameof(model.WaivedAmount), $"Waived Amount cannot exceed Total Surcharge (PKR {model.TotalAmount:N0}).");

            if (!ModelState.IsValid)
                return View(model);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var statuses = ViewBag.WorkflowStatuses as List<string> ?? new List<string> { "Initiated", "Approved", "Declined" };
            var initiatedStatus = ResolveWorkflowStatus(statuses, "Initiated", "Initialted", statuses[0]);
            model.WaiverID = await GenerateWaiverIdAsync();
            model.WaiverType = string.IsNullOrWhiteSpace(model.WaiverType) ? "Surcharge Waiver" : model.WaiverType;
            model.AccountHead = string.IsNullOrWhiteSpace(model.AccountHead) ? SurchargeWaivedOffAccountHead : model.AccountHead.Trim();
            model.Status = initiatedStatus;
            model.CreatedBy = userId;
            model.LastModifiedBy = userId;
            model.CreatedAt = DateTime.Now;

            _context.Waivers.Add(model);
            _context.ActivityLogs.Add(new ActivityLog
            {
                UserID = userId,
                Action = $"Waiver {model.WaiverID} initiated for customer {model.CustomerID}. Amount: PKR {model.WaivedAmount:N0}",
                RefType = "Waiver",
                RefID = model.WaiverID,
                CreatedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Waiver {model.WaiverID} created successfully.";
            return RedirectToAction(nameof(Details), new { id = model.WaiverID });
        }

        public async Task<IActionResult> Details(string id)
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;
            await SetWorkflowViewBagAsync();

            var waiver = await _context.Waivers
                .Include(w => w.Customer)
                    .ThenInclude(c => c!.PaymentPlan)
                        .ThenInclude(p => p!.Project)
                .Include(w => w.CreatedByUser)
                .Include(w => w.ApprovedByUser)
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.WaiverID == id);
            if (waiver == null) return NotFound();
            return View(waiver);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
            await SetWorkflowViewBagAsync();

            var waiver = await _context.Waivers
                .Include(w => w.Customer)
                    .ThenInclude(c => c!.PaymentPlan)
                        .ThenInclude(p => p!.Project)
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.WaiverID == id);
            if (waiver == null) return NotFound();

            var workflowStatuses = ViewBag.WorkflowStatuses as List<string> ?? new List<string> { "Initiated", "Approved", "Declined" };
            var approvedStatus = ResolveWorkflowStatus(workflowStatuses, "Approved");
            if (string.Equals(waiver.Status, approvedStatus, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Approved waivers cannot be edited.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var currentIndex = GetWorkflowIndex(workflowStatuses, waiver.Status);
            ViewBag.CurrentWorkflowIndex = currentIndex;
            ViewBag.HasPreviousStep = currentIndex > 0;
            ViewBag.HasNextStep = currentIndex >= 0 && currentIndex < workflowStatuses.Count - 1;
            ViewBag.PreviousStepLabel = currentIndex > 0 ? workflowStatuses[currentIndex - 1] : null;
            ViewBag.NextStepLabel = currentIndex >= 0 && currentIndex < workflowStatuses.Count - 1 ? workflowStatuses[currentIndex + 1] : null;
            return View(waiver);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Waiver model)
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
            await SetWorkflowViewBagAsync();

            var existing = await _context.Waivers.FirstOrDefaultAsync(w => w.WaiverID == id);
            if (existing == null) return NotFound();

            var statuses = ViewBag.WorkflowStatuses as List<string> ?? new List<string> { "Initiated", "Approved", "Declined" };
            var approvedStatus = ResolveWorkflowStatus(statuses, "Approved");
            if (string.Equals(existing.Status, approvedStatus, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Approved waivers cannot be edited.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (string.IsNullOrWhiteSpace(model.Comments))
                ModelState.AddModelError(nameof(model.Comments), "Comments are required.");
            if (model.WaivedPercentage.HasValue && (model.WaivedPercentage.Value < 0m || model.WaivedPercentage.Value > 100m))
                ModelState.AddModelError(nameof(model.WaivedPercentage), "Waived Percentage must be between 0 and 100.");
            if (model.WaivedAmount <= 0m)
                ModelState.AddModelError(nameof(model.WaivedAmount), "Waived Amount must be greater than zero.");
            if (existing.TotalAmount > 0m && model.WaivedAmount > existing.TotalAmount)
                ModelState.AddModelError(nameof(model.WaivedAmount), "Waived Amount cannot exceed Total Surcharge.");

            if (!ModelState.IsValid)
            {
                model.WaiverID = existing.WaiverID;
                model.CustomerID = existing.CustomerID;
                model.TotalAmount = existing.TotalAmount;
                model.Status = existing.Status;
                model.WaiverType = existing.WaiverType;
                model.AccountHead = existing.AccountHead;
                model.CreatedAt = existing.CreatedAt;
                return View(model);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            existing.Comments = model.Comments;
            existing.WaivedAmount = model.WaivedAmount;
            existing.WaivedPercentage = model.WaivedPercentage;
            existing.AccountHead = string.IsNullOrWhiteSpace(model.AccountHead) ? existing.AccountHead : model.AccountHead;
            existing.LastModifiedBy = userId;

            _context.ActivityLogs.Add(new ActivityLog
            {
                UserID = userId,
                Action = $"Waiver {existing.WaiverID} edited.",
                RefType = "Waiver",
                RefID = existing.WaiverID,
                CreatedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Waiver {existing.WaiverID} updated successfully.";
            return RedirectToAction(nameof(Details), new { id = existing.WaiverID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveStatus(string id, string direction)
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
            await SetWorkflowViewBagAsync();

            var waiver = await _context.Waivers.FirstOrDefaultAsync(w => w.WaiverID == id);
            if (waiver == null)
            {
                TempData["ErrorMessage"] = "Waiver not found.";
                return RedirectToAction(nameof(Index));
            }

            var workflowStatuses = ViewBag.WorkflowStatuses as List<string> ?? new List<string> { "Initiated", "Approved", "Declined" };
            var approvedStatus = ResolveWorkflowStatus(workflowStatuses, "Approved");
            if (string.Equals(waiver.Status, approvedStatus, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Approved waivers cannot be moved.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var idx = GetWorkflowIndex(workflowStatuses, waiver.Status);
            if (idx < 0)
            {
                TempData["ErrorMessage"] = "Current waiver status is not part of configured workflow.";
                return RedirectToAction(nameof(Edit), new { id });
            }

            var requestedForward = string.Equals(direction, "forward", StringComparison.OrdinalIgnoreCase);
            var requestedBackward = string.Equals(direction, "backward", StringComparison.OrdinalIgnoreCase);
            if (!requestedForward && !requestedBackward)
            {
                TempData["ErrorMessage"] = "Invalid status move direction.";
                return RedirectToAction(nameof(Edit), new { id });
            }

            var targetIndex = requestedForward ? idx + 1 : idx - 1;
            if (targetIndex < 0 || targetIndex >= workflowStatuses.Count)
            {
                TempData["ErrorMessage"] = "No further workflow step available in that direction.";
                return RedirectToAction(nameof(Edit), new { id });
            }

            var oldStatus = waiver.Status ?? string.Empty;
            var targetStatus = workflowStatuses[targetIndex];
            waiver.Status = targetStatus;
            waiver.LastModifiedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.Equals(targetStatus, approvedStatus, StringComparison.OrdinalIgnoreCase))
            {
                await ApplyApprovedStateAsync(waiver);
            }

            _context.ActivityLogs.Add(new ActivityLog
            {
                UserID = waiver.LastModifiedBy,
                Action = $"Waiver {waiver.WaiverID} status changed from {oldStatus} to {targetStatus}.",
                RefType = "Waiver",
                RefID = waiver.WaiverID,
                CreatedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Waiver {waiver.WaiverID} moved to {targetStatus}.";
            return string.Equals(targetStatus, approvedStatus, StringComparison.OrdinalIgnoreCase)
                ? RedirectToAction(nameof(Details), new { id = waiver.WaiverID })
                : RedirectToAction(nameof(Edit), new { id = waiver.WaiverID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var denied = await EnsurePermissionAsync("Admin");
            if (denied != null) return denied;
            await SetWorkflowViewBagAsync();

            var waiver = await _context.Waivers.FirstOrDefaultAsync(w => w.WaiverID == id);
            if (waiver == null)
            {
                TempData["ErrorMessage"] = "Waiver not found.";
                return RedirectToAction(nameof(Index));
            }

            var statuses = ViewBag.WorkflowStatuses as List<string> ?? new List<string> { "Initiated", "Approved", "Declined" };
            var initiatedStatus = ResolveWorkflowStatus(statuses, "Initiated", "Initialted", statuses[0]);
            if (!string.Equals(waiver.Status, initiatedStatus, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Only initiated waivers can be deleted.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _context.Waivers.Remove(waiver);
            _context.ActivityLogs.Add(new ActivityLog
            {
                UserID = userId,
                Action = $"Waiver {waiver.WaiverID} deleted.",
                RefType = "Waiver",
                RefID = waiver.WaiverID,
                CreatedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Waiver {waiver.WaiverID} deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task ApplyApprovedStateAsync(Waiver waiver)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            waiver.ApprovedBy = userId;
            waiver.ApprovedAt = DateTime.Now;

            var alreadyExists = await _context.Payments.AnyAsync(p =>
                p.CustomerID == waiver.CustomerID
                && p.ScheduleID == null
                && p.ReferenceNo == waiver.WaiverID
                && p.Method == "Waiver");
            if (alreadyExists)
            {
                return;
            }

            var payment = new Payment
            {
                PaymentID = GenerateID(),
                CustomerID = waiver.CustomerID,
                ScheduleID = null,
                PaymentDate = DateTime.Now,
                Amount = -Math.Abs(waiver.WaivedAmount),
                AccountHead = string.IsNullOrWhiteSpace(waiver.AccountHead) ? SurchargeWaivedOffAccountHead : waiver.AccountHead.Trim(),
                Method = "Waiver",
                ReferenceNo = waiver.WaiverID,
                Status = "Waived",
                Remarks = $"Waiver approved ({waiver.WaiverType}) | AccountHead:{(string.IsNullOrWhiteSpace(waiver.AccountHead) ? SurchargeWaivedOffAccountHead : waiver.AccountHead.Trim())}",
                AuditStatus = "Approved"
            };
            _context.Payments.Add(payment);
        }

        private async Task<string> GenerateWaiverIdAsync()
        {
            var last = await _context.Waivers
                .OrderByDescending(w => w.WaiverID)
                .Select(w => w.WaiverID)
                .FirstOrDefaultAsync();
            var next = 1;
            if (!string.IsNullOrEmpty(last))
            {
                var numPart = new string(last.Where(char.IsDigit).ToArray());
                if (int.TryParse(numPart, out var n)) next = n + 1;
            }
            return $"WVR{next:D7}";
        }

        private static readonly string[] AllowedAttachmentExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".pdf" };
        private const long MaxAttachmentSize = 8 * 1024 * 1024;

        [HttpPost]
        public async Task<IActionResult> UploadAttachment(string waiverId, IFormFile file, string description = "")
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;

            try
            {
                var normalizedWaiverId = NormalizeId(waiverId);
                if (string.IsNullOrEmpty(normalizedWaiverId))
                    return Json(new { success = false, message = "Waiver ID is required." });
                if (file == null || file.Length == 0)
                    return Json(new { success = false, message = "Please select a file to upload." });

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedAttachmentExtensions.Contains(ext))
                    return Json(new { success = false, message = "Only image files and PDF are allowed." });
                if (file.Length > MaxAttachmentSize)
                    return Json(new { success = false, message = "File size exceeds 8MB limit." });

                var waiver = await _context.Waivers.FindAsync(normalizedWaiverId);
                if (waiver == null)
                    return Json(new { success = false, message = "Waiver not found." });

                var safeId = SanitizePathSegment(normalizedWaiverId);
                if (string.IsNullOrEmpty(safeId))
                    return Json(new { success = false, message = "Invalid Waiver ID for file storage." });

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "waivers", safeId);
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                var relativePath = $"/uploads/waivers/{safeId}/{uniqueFileName}";

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await file.CopyToAsync(stream);

                var attachment = new Attachment
                {
                    AttachmentID = GenerateID(),
                    RefType = "Waiver",
                    RefID = normalizedWaiverId,
                    AttachmentType = "Waiver",
                    FileName = file.FileName,
                    FilePath = relativePath,
                    FileSize = file.Length,
                    FileType = file.ContentType,
                    Description = description,
                    UploadedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    UploadedAt = DateTime.Now
                };
                _context.Attachments.Add(attachment);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "File uploaded successfully",
                    attachment = new
                    {
                        attachmentID = attachment.AttachmentID,
                        fileName = attachment.FileName,
                        filePath = attachment.FilePath,
                        fileSize = attachment.FileSize,
                        fileType = attachment.FileType,
                        attachmentType = attachment.AttachmentType,
                        description = attachment.Description,
                        uploadedAt = attachment.UploadedAt.ToString("MMM dd, yyyy hh:mm tt")
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAttachments(string waiverId)
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;

            try
            {
                var normalizedWaiverId = NormalizeId(waiverId);
                if (string.IsNullOrEmpty(normalizedWaiverId))
                    return Json(new { success = false, message = "Waiver ID is required." });

                var attachments = await _context.Attachments
                    .Where(a => a.RefType == "Waiver" && a.RefID == normalizedWaiverId)
                    .OrderByDescending(a => a.UploadedAt)
                    .Select(a => new
                    {
                        attachmentID = a.AttachmentID,
                        fileName = a.FileName,
                        filePath = a.FilePath,
                        fileSize = a.FileSize,
                        fileType = a.FileType,
                        attachmentType = a.AttachmentType,
                        description = a.Description,
                        uploadedAt = a.UploadedAt.ToString("MMM dd, yyyy hh:mm tt")
                    })
                    .ToListAsync();

                return Json(new { success = true, attachments });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAttachment(string attachmentId)
        {
            var denied = await EnsurePermissionAsync("Admin");
            if (denied != null) return denied;

            try
            {
                var attachment = await _context.Attachments
                    .FirstOrDefaultAsync(a => a.AttachmentID == attachmentId && a.RefType == "Waiver");
                if (attachment == null)
                    return Json(new { success = false, message = "Attachment not found." });

                if (!string.IsNullOrEmpty(attachment.FilePath))
                {
                    var trimmedPath = attachment.FilePath.TrimStart('~').TrimStart('/');
                    var fullPath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        trimmedPath.Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(fullPath))
                        System.IO.File.Delete(fullPath);
                }

                _context.Attachments.Remove(attachment);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Attachment deleted." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private static string NormalizeId(string? id) => (id ?? string.Empty).Trim();

        private static string SanitizePathSegment(string? segment)
        {
            var s = NormalizeId(segment);
            if (string.IsNullOrEmpty(s)) return string.Empty;

            foreach (var c in Path.GetInvalidFileNameChars())
            {
                s = s.Replace(c, '_');
            }

            s = s.Replace(Path.DirectorySeparatorChar, '_').Replace(Path.AltDirectorySeparatorChar, '_');
            return s.Trim().TrimEnd('.');
        }

        private static string GenerateID() => Guid.NewGuid().ToString("N")[..10].ToUpper();
    }
}
