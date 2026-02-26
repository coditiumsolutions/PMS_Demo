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
    public class TransferController : Controller
    {
        private const string ModuleKey = "Transfer";
        private readonly PMSDbContext _context;
        private readonly IModulePermissionService _modulePermission;
        private const string WorkflowCreated = "Created";
        private const string WorkflowAtAccounts = "At the Desk of Accounts";
        private const string WorkflowAtTransferApproval = "At the Desk of Transfer Approval";
        private const string WorkflowApproved = "Approved";

        public TransferController(PMSDbContext context, IModulePermissionService modulePermission)
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

        private static string NormalizeId(string? id)
        {
            return (id ?? string.Empty).Trim();
        }

        private static string SanitizePathSegment(string? segment)
        {
            // Windows does not allow trailing spaces/dots in folder names and disallows certain characters.
            var s = NormalizeId(segment);
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }

            foreach (var c in Path.GetInvalidFileNameChars())
            {
                s = s.Replace(c, '_');
            }

            // Extra safety for path separators.
            s = s.Replace(Path.DirectorySeparatorChar, '_')
                 .Replace(Path.AltDirectorySeparatorChar, '_');

            // Windows also rejects trailing dots/spaces.
            s = s.Trim().TrimEnd('.');
            return s;
        }

        public async Task<IActionResult> Index(string customerIdFilter = "", string workflowFilter = "")
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;
            var query = _context.Transfers
                .Include(t => t.Customer)
                    .ThenInclude(c => c!.PaymentPlan)
                        .ThenInclude(p => p!.Project)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(customerIdFilter))
                query = query.Where(t => t.CustomerID.Contains(customerIdFilter));

            if (!string.IsNullOrWhiteSpace(workflowFilter))
                query = query.Where(t => t.WorkFlowStatus == workflowFilter);

            var list = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();

            ViewBag.CustomerIdFilter = customerIdFilter ?? "";
            ViewBag.WorkflowFilter = workflowFilter ?? "";
            ViewBag.WorkflowStatuses = new[] { WorkflowCreated, WorkflowAtAccounts, WorkflowAtTransferApproval, WorkflowApproved };
            return View(list);
        }

        public async Task<IActionResult> Create(string? customerId = null)
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
            ViewBag.WorkflowStatuses = new[] { WorkflowCreated };
            SetCitiesAndCountriesViewBag();
            SetPaymentMethodsViewBag();
            var model = new Transfer { WorkFlowStatus = WorkflowCreated, CreatedAt = DateTime.Now };
            if (!string.IsNullOrWhiteSpace(customerId))
            {
                var customer = await _context.Customers.FindAsync(customerId);
                if (customer != null)
                {
                    model.CustomerID = customer.CustomerID;
                    model.SellerName = customer.FullName;
                    model.SellerFatherName = customer.FatherName;
                    model.SellerCNIC = customer.CNIC;
                    model.SellerContact = customer.Phone;
                    model.SellerAddress = customer.MailingAddress ?? customer.PermanentAddress;
                }
            }
            return View(model);
        }

        private void SetCitiesAndCountriesViewBag()
        {
            var citiesConfig = _context.Configurations.FirstOrDefault(c => c.ConfigKey == "cities");
            ViewBag.Cities = citiesConfig?.ConfigValue != null
                ? citiesConfig.ConfigValue.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList()
                : new List<string>();
            var countriesConfig = _context.Configurations.FirstOrDefault(c => c.ConfigKey == "countries");
            ViewBag.Countries = countriesConfig?.ConfigValue != null
                ? countriesConfig.ConfigValue.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList()
                : new List<string>();
        }

        private void SetPaymentMethodsViewBag()
        {
            var config = _context.Configurations.FirstOrDefault(c => c.ConfigKey == "paymentmethods" || c.ConfigKey == "PaymentMethods");
            ViewBag.PaymentMethods = config?.ConfigValue != null
                ? config.ConfigValue.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList()
                : new List<string> { "DD/DS", "Cash", "Bank Transfer", "Cheque", "Online", "Mobile Money" };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Transfer model)
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
            model.TransferID = await GenerateTransferIdAsync();
            model.WorkFlowStatus ??= WorkflowCreated;
            model.CreatedAt = DateTime.Now;

            if (await _context.Customers.AllAsync(c => c.CustomerID != model.CustomerID))
            {
                ModelState.AddModelError("CustomerID", "Customer ID not found.");
            }
            else
            {
                var today = DateTime.Today;
                var hasActiveNDC = await _context.NDCs
                    .AnyAsync(n => n.CustomerID == model.CustomerID.Trim()
                        && n.NDCExpiryDate.HasValue
                        && n.IssuedDate.Date <= today
                        && n.NDCExpiryDate.Value.Date >= today);
                if (!hasActiveNDC)
                {
                    ModelState.AddModelError("CustomerID", "No active NDC found for this customer. Transfer requires an NDC where current date is between Issue Date and Expiry Date.");
                }
            }

            if (string.IsNullOrWhiteSpace(model.BuyerName))
                ModelState.AddModelError("BuyerName", "Buyer Name is required.");
            if (string.IsNullOrWhiteSpace(model.BuyerFatherName))
                ModelState.AddModelError("BuyerFatherName", "Buyer Father Name is required.");
            if (string.IsNullOrWhiteSpace(model.BuyerCNIC))
                ModelState.AddModelError("BuyerCNIC", "Buyer CNIC is required.");
            else if (!System.Text.RegularExpressions.Regex.IsMatch(model.BuyerCNIC.Trim(), @"^\d{5}-\d{7}-\d$"))
                ModelState.AddModelError("BuyerCNIC", "Buyer CNIC must be in format XXXXX-XXXXXXX-X (5 digits, hyphen, 7 digits, hyphen, 1 digit).");
            if (string.IsNullOrWhiteSpace(model.BuyerContact))
                ModelState.AddModelError("BuyerContact", "Buyer Contact is required.");
            if (string.IsNullOrWhiteSpace(model.BuyerAddress))
                ModelState.AddModelError("BuyerAddress", "Buyer Address is required.");
            if (string.IsNullOrWhiteSpace(model.BuyerCity))
                ModelState.AddModelError("BuyerCity", "Buyer City is required.");
            if (string.IsNullOrWhiteSpace(model.BuyerCountry))
                ModelState.AddModelError("BuyerCountry", "Buyer Country is required.");

            if (model.TransferFeeDue == null)
                ModelState.AddModelError("TransferFeeDue", "Transfer Fee Due is required.");
            if (model.TransferFeePaid == null)
                ModelState.AddModelError("TransferFeePaid", "Transfer Fee Paid is required.");
            if (!model.PaymentDate.HasValue)
                ModelState.AddModelError("PaymentDate", "Payment Date is required.");
            if (string.IsNullOrWhiteSpace(model.PaymentMode))
                ModelState.AddModelError("PaymentMode", "Payment Mode is required.");
            if (string.IsNullOrWhiteSpace(model.PaymentChallanNo))
                ModelState.AddModelError("PaymentChallanNo", "Challan No is required.");

            if (string.Equals(model.WorkFlowStatus, WorkflowApproved, StringComparison.OrdinalIgnoreCase))
            {
                var due = model.TransferFeeDue ?? 0;
                var paid = model.TransferFeePaid ?? 0;
                if (Math.Abs(due - paid) > 0.001)
                    ModelState.AddModelError("", "When Workflow Status is Approved, Transfer Fee Due must equal Transfer Fee Paid.");
            }

            if (ModelState.IsValid)
            {
                _context.Transfers.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = model.TransferID });
            }

            ViewBag.WorkflowStatuses = new[] { WorkflowCreated };
            SetCitiesAndCountriesViewBag();
            SetPaymentMethodsViewBag();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> GetCustomerForTransfer(string customerId)
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;
            if (string.IsNullOrWhiteSpace(customerId))
                return Json(new { success = false, message = "Customer ID is required." });

            var customer = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CustomerID == customerId);

            if (customer == null)
                return Json(new { success = false, message = "Customer not found." });

            var today = DateTime.Today;
            var activeNDC = await _context.NDCs
                .AsNoTracking()
                .Where(n => n.CustomerID == customerId.Trim()
                    && n.NDCExpiryDate.HasValue
                    && n.IssuedDate.Date <= today
                    && n.NDCExpiryDate.Value.Date >= today)
                .FirstOrDefaultAsync();

            var hasActiveNDC = activeNDC != null;
            var ndcMessage = hasActiveNDC ? null : "No active NDC found. Transfer requires an NDC where current date is between Issue Date and Expiry Date.";

            return Json(new
            {
                success = true,
                customerID = customer.CustomerID,
                sellerName = customer.FullName,
                sellerFatherName = customer.FatherName,
                sellerCNIC = customer.CNIC,
                sellerContact = customer.Phone,
                sellerAddress = customer.MailingAddress ?? customer.PermanentAddress,
                hasActiveNDC,
                ndcMessage
            });
        }

        public async Task<IActionResult> Details(string id)
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;
            id = NormalizeId(id);
            if (string.IsNullOrEmpty(id)) return NotFound();

            var transfer = await _context.Transfers
                .Include(t => t.Customer)
                    .ThenInclude(c => c!.PaymentPlan)
                        .ThenInclude(p => p!.Project)
                .FirstOrDefaultAsync(t => t.TransferID == id);

            if (transfer == null) return NotFound();

            ViewBag.WorkflowStatuses = new[] { WorkflowCreated, WorkflowAtAccounts, WorkflowAtTransferApproval, WorkflowApproved };
            return View(transfer);
        }

        /// <summary>
        /// Printable transfer letter (proof of transfer) for approved transfers. Can be printed and handed to the new buyer.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> TransferLetter(string id)
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;
            id = NormalizeId(id);
            if (string.IsNullOrEmpty(id)) return NotFound();

            var transfer = await _context.Transfers
                .Include(t => t.Customer)
                    .ThenInclude(c => c!.PaymentPlan)
                        .ThenInclude(p => p!.Project)
                .FirstOrDefaultAsync(t => t.TransferID == id);

            if (transfer == null) return NotFound();

            if (transfer.WorkFlowStatus != WorkflowApproved)
            {
                TempData["ErrorMessage"] = "Transfer letter is only available for approved transfers.";
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(transfer);
        }

        public async Task<IActionResult> TransferReceipt(string id)
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;
            id = NormalizeId(id);
            if (string.IsNullOrEmpty(id)) return NotFound();

            var transfer = await _context.Transfers
                .Include(t => t.Customer)
                    .ThenInclude(c => c!.PaymentPlan)
                        .ThenInclude(p => p!.Project)
                .FirstOrDefaultAsync(t => t.TransferID == id);

            if (transfer == null) return NotFound();

            if (transfer.WorkFlowStatus == WorkflowApproved)
            {
                TempData["ErrorMessage"] = "This transfer is already approved. Use Transfer Letter instead.";
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(transfer);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
            id = NormalizeId(id);
            if (string.IsNullOrEmpty(id)) return NotFound();

            var transfer = await _context.Transfers
                .Include(t => t.Customer)
                .FirstOrDefaultAsync(t => t.TransferID == id);

            if (transfer == null) return NotFound();

            if (transfer.WorkFlowStatus == WorkflowApproved)
            {
                TempData["ErrorMessage"] = "This transfer is approved and cannot be edited.";
                return RedirectToAction(nameof(Details), new { id });
            }

            ViewBag.WorkflowStatuses = new[] { WorkflowCreated, WorkflowAtAccounts, WorkflowAtTransferApproval, WorkflowApproved };
            SetPaymentMethodsViewBag();
            return View(transfer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Transfer model)
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
            id = NormalizeId(id);
            model.TransferID = NormalizeId(model.TransferID);
            if (string.IsNullOrEmpty(id) || id != model.TransferID) return NotFound();

            var existing = await _context.Transfers
                .Include(t => t.Customer)
                .FirstOrDefaultAsync(t => t.TransferID == id);

            if (existing == null) return NotFound();

            if (existing.WorkFlowStatus == WorkflowApproved)
            {
                TempData["ErrorMessage"] = "This transfer is approved and cannot be edited.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (string.Equals(model.WorkFlowStatus, WorkflowApproved, StringComparison.OrdinalIgnoreCase))
            {
                var due = model.TransferFeeDue ?? 0;
                var paid = model.TransferFeePaid ?? 0;
                if (Math.Abs(due - paid) > 0.001)
                {
                    ModelState.AddModelError("", "When Workflow Status is Approved, Transfer Fee Due must equal Transfer Fee Paid.");
                    ViewBag.WorkflowStatuses = new[] { WorkflowCreated, WorkflowAtAccounts, WorkflowAtTransferApproval, WorkflowApproved };
                    SetPaymentMethodsViewBag();
                    return View(model);
                }
            }

            existing.WorkFlowStatus = model.WorkFlowStatus;
            existing.SellerName = model.SellerName;
            existing.SellerFatherName = model.SellerFatherName;
            existing.SellerCNIC = model.SellerCNIC;
            existing.SellerContact = model.SellerContact;
            existing.SellerAddress = model.SellerAddress;
            existing.BuyerName = model.BuyerName;
            existing.BuyerFatherName = model.BuyerFatherName;
            existing.BuyerCNIC = model.BuyerCNIC;
            existing.BuyerContact = model.BuyerContact;
            existing.BuyerAddress = model.BuyerAddress;
            existing.BuyerCity = model.BuyerCity;
            existing.BuyerCountry = model.BuyerCountry;
            existing.BuyerAttachments = model.BuyerAttachments;
            existing.SellerAttachments = model.SellerAttachments;
            existing.SellerBiometric = model.SellerBiometric;
            existing.BuyerBiometric = model.BuyerBiometric;
            existing.TransferFeeDue = model.TransferFeeDue;
            existing.TransferFeePaid = model.TransferFeePaid;
            existing.PaymentDate = model.PaymentDate;
            existing.PaymentMode = model.PaymentMode;
            existing.PaymentChallanNo = model.PaymentChallanNo;
            existing.Details = model.Details;
            existing.CROComments = model.CROComments;
            existing.AccountsComments = model.AccountsComments;
            existing.TransferComments = model.TransferComments;

            if (model.WorkFlowStatus == WorkflowApproved)
            {
                var customer = existing.Customer;
                if (customer != null)
                {
                    customer.FullName = model.BuyerName ?? customer.FullName;
                    customer.FatherName = model.BuyerFatherName ?? customer.FatherName;
                    customer.CNIC = model.BuyerCNIC ?? customer.CNIC;
                    customer.Phone = model.BuyerContact ?? customer.Phone;
                    customer.MailingAddress = model.BuyerAddress ?? customer.MailingAddress;
                    customer.PermanentAddress = model.BuyerAddress ?? customer.PermanentAddress;
                    customer.City = model.BuyerCity ?? customer.City;
                    customer.Country = model.BuyerCountry ?? customer.Country;
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task<string> GenerateTransferIdAsync()
        {
            var prefix = "TRF-";
            var today = DateTime.Today.ToString("yyyyMMdd");
            var existing = await _context.Transfers
                .Where(t => t.TransferID.StartsWith(prefix + today))
                .OrderByDescending(t => t.TransferID)
                .Select(t => t.TransferID)
                .FirstOrDefaultAsync();

            int seq = 1;
            if (!string.IsNullOrEmpty(existing) && existing.Length >= prefix.Length + today.Length + 2)
            {
                var part = existing[(prefix.Length + today.Length)..].TrimStart('-');
                if (int.TryParse(part, out var n)) seq = n + 1;
            }
            return prefix + today + "-" + seq.ToString("D4");
        }

        // ========== Attachment management (file upload like Customer module) ==========
        private static readonly string[] AllowedAttachmentExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".pdf" };
        private const long MaxAttachmentSize = 8 * 1024 * 1024; // 8MB

        [HttpPost]
        public async Task<IActionResult> UploadAttachment(string transferId, IFormFile file, string attachmentType, string description = "")
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
            try
            {
                var normalizedTransferId = NormalizeId(transferId);
                if (string.IsNullOrEmpty(normalizedTransferId))
                    return Json(new { success = false, message = "Transfer ID is required." });
                if (file == null || file.Length == 0)
                    return Json(new { success = false, message = "Please select a file to upload." });
                if (string.IsNullOrEmpty(attachmentType) || (attachmentType != "Seller" && attachmentType != "Buyer"))
                    return Json(new { success = false, message = "Attachment type must be Seller or Buyer." });

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedAttachmentExtensions.Contains(ext))
                    return Json(new { success = false, message = "Only image files (JPG, PNG, GIF, BMP) and PDF are allowed." });
                if (file.Length > MaxAttachmentSize)
                    return Json(new { success = false, message = "File size exceeds 8MB limit." });

                var transfer = await _context.Transfers.FindAsync(normalizedTransferId);
                if (transfer == null)
                    return Json(new { success = false, message = "Transfer not found." });

                var safeId = SanitizePathSegment(normalizedTransferId);
                if (string.IsNullOrEmpty(safeId))
                {
                    return Json(new { success = false, message = "Invalid Transfer ID for file storage." });
                }
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "transfers", safeId);
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                var relativePath = $"/uploads/transfers/{safeId}/{uniqueFileName}";

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await file.CopyToAsync(stream);

                var attachmentId = Guid.NewGuid().ToString("N")[..10].ToUpper();
                var uploadedByRaw = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var uploadedBy = string.IsNullOrEmpty(uploadedByRaw) ? null
                    : (uploadedByRaw.Length <= 10 ? uploadedByRaw : uploadedByRaw.Substring(0, 10));

                var attachment = new Attachment
                {
                    AttachmentID = attachmentId,
                    RefType = "Transfer",
                    RefID = normalizedTransferId,
                    AttachmentType = attachmentType,
                    FileName = file.FileName,
                    FilePath = relativePath,
                    FileSize = file.Length,
                    FileType = file.ContentType,
                    Description = description,
                    UploadedBy = uploadedBy,
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
                var msg = ex.Message;
                if (ex.InnerException != null)
                    msg += " " + ex.InnerException.Message;
                return Json(new { success = false, message = msg });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAttachments(string transferId)
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;
            try
            {
                var normalizedTransferId = NormalizeId(transferId);
                if (string.IsNullOrEmpty(normalizedTransferId))
                    return Json(new { success = false, message = "Transfer ID is required." });

                var attachments = await _context.Attachments
                    .Where(a => a.RefType == "Transfer" && a.RefID == normalizedTransferId)
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
                if (string.IsNullOrEmpty(attachmentId))
                    return Json(new { success = false, message = "Attachment ID is required." });

                var attachment = await _context.Attachments
                    .FirstOrDefaultAsync(a => a.AttachmentID == attachmentId && a.RefType == "Transfer");
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
    }
}
