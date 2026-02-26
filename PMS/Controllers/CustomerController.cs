using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS.Data;
using PMS.Models;
using PMS.Services;
using System.Security.Claims;
using System.IO;
using System;
using System.Linq;

namespace PMS.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private const string ModuleKey = "Customer";
        private readonly PMSDbContext _context;
        private readonly IModulePermissionService _modulePermission;

        private static readonly string[] _allowedKinFileExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".pdf" };
        private const long _maxKinFileSize = 8 * 1024 * 1024; // 8MB

        public CustomerController(PMSDbContext context, IModulePermissionService modulePermission)
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

        public async Task<IActionResult> Index(string projectFilter = "All", string statusFilter = "All", string searchTerm = "")
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;
            // Get all projects for dropdown
            var projects = await _context.Projects
                .OrderBy(p => p.ProjectName)
                .Select(p => new { p.ProjectID, p.ProjectName })
                .ToListAsync();
            ViewBag.Projects = projects;
            ViewBag.ProjectFilter = projectFilter;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.SearchTerm = searchTerm;

            // Build query
            var query = _context.Customers
                .Include(c => c.Registration)
                .Include(c => c.PaymentPlan)
                    .ThenInclude(p => p.Project)
                .Include(c => c.Allotments)
                .AsQueryable();

            // Apply project filter
            if (!string.IsNullOrEmpty(projectFilter) && projectFilter != "All")
            {
                query = query.Where(c => c.PaymentPlan != null && c.PaymentPlan.ProjectID == projectFilter);
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
            {
                query = query.Where(c => c.Status == statusFilter);
            }

            // Apply search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(c => 
                    (c.CustomerID != null && c.CustomerID.ToLower().Contains(searchTerm)) ||
                    (c.FullName != null && c.FullName.ToLower().Contains(searchTerm)) ||
                    (c.CNIC != null && c.CNIC.ToLower().Contains(searchTerm)) ||
                    (c.Phone != null && c.Phone.ToLower().Contains(searchTerm)) ||
                    (c.Email != null && c.Email.ToLower().Contains(searchTerm))
                );
            }

            var customers = await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(customers);
        }

        public async Task<IActionResult> ByProject()
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;
            // Get project summary with counts only (no customer data)
            var projectSummary = await _context.Customers
                .Include(c => c.PaymentPlan)
                    .ThenInclude(p => p.Project)
                .Where(c => c.PaymentPlan != null && c.PaymentPlan.Project != null)
                .GroupBy(c => new { 
                    ProjectName = c.PaymentPlan.Project.ProjectName,
                    ProjectID = c.PaymentPlan.Project.ProjectID
                })
                .Select(g => new {
                    ProjectName = g.Key.ProjectName,
                    ProjectID = g.Key.ProjectID,
                    TotalCustomers = g.Count(),
                    SizeCounts = g.GroupBy(c => c.RegisteredSize ?? "Unknown")
                        .Select(s => new {
                            Size = s.Key,
                            Count = s.Count()
                        })
                        .OrderBy(s => s.Size)
                        .ToList()
                })
                .OrderBy(g => g.ProjectName)
                .ToListAsync();

            return View(projectSummary);
        }

        /// <summary>Customer reports: allotted vs not allotted, by status, by month, by city, etc.</summary>
        public async Task<IActionResult> Reports()
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;
            var customers = await _context.Customers
                .Include(c => c.PaymentPlan)
                    .ThenInclude(p => p.Project)
                .Include(c => c.Allotments)
                .ToListAsync();

            var summary = new CustomerReportsSummary
            {
                TotalCustomers = customers.Count,
                TotalAllotted = customers.Count(c => c.Allotments != null && c.Allotments.Any()),
                TotalNotAllotted = customers.Count(c => c.Allotments == null || !c.Allotments.Any()),
                ActiveCustomers = customers.Count(c => (c.Status ?? "").Equals("Active", StringComparison.OrdinalIgnoreCase)),
                InactiveCustomers = customers.Count(c => !(c.Status ?? "").Equals("Active", StringComparison.OrdinalIgnoreCase))
            };

            // 1. Allotted vs Not Allotted per Project
            var allottedPerProject = customers
                .Where(c => c.PaymentPlan?.Project != null)
                .GroupBy(c => new { c.PaymentPlan!.Project!.ProjectID, ProjectName = c.PaymentPlan.Project.ProjectName ?? "Unknown" })
                .Select(g => new AllottedPerProjectReportItem
                {
                    ProjectID = g.Key.ProjectID,
                    ProjectName = g.Key.ProjectName,
                    Allotted = g.Count(c => c.Allotments != null && c.Allotments.Any()),
                    NotAllotted = g.Count(c => c.Allotments == null || !c.Allotments.Any())
                })
                .OrderBy(x => x.ProjectName)
                .ToList();

            // 2. Customers per Project grouped by Status
            var byStatusPerProject = customers
                .Where(c => c.PaymentPlan?.Project != null)
                .GroupBy(c => new { c.PaymentPlan!.Project!.ProjectID, ProjectName = c.PaymentPlan.Project.ProjectName ?? "Unknown", Status = c.Status ?? "Unknown" })
                .Select(g => new CustomersByStatusPerProjectItem
                {
                    ProjectID = g.Key.ProjectID,
                    ProjectName = g.Key.ProjectName,
                    Status = g.Key.Status,
                    Count = g.Count()
                })
                .OrderBy(x => x.ProjectName)
                .ThenBy(x => x.Status)
                .ToList();

            // 3. New Customers each month (last 24 months)
            var cutoff = DateTime.Today.AddMonths(-24);
            var newCustomersPerMonth = customers
                .Where(c => c.CreatedAt >= cutoff)
                .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month })
                .Select(g => new NewCustomersPerMonthItem
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    YearMonthLabel = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("yyyy-MM"),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
                .ToList();

            // 4. Customers by City
            var byCity = customers
                .GroupBy(c => string.IsNullOrWhiteSpace(c.City) ? "(Not set)" : (c.City ?? ""))
                .Select(g => new CustomersByCityItem { City = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(20)
                .ToList();

            // 5. Customers by Registered Size
            var bySize = customers
                .GroupBy(c => string.IsNullOrWhiteSpace(c.RegisteredSize) ? "(Not set)" : (c.RegisteredSize ?? ""))
                .Select(g => new CustomersBySizeItem { Size = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            ViewBag.Summary = summary;
            ViewBag.AllottedPerProject = allottedPerProject;
            ViewBag.ByStatusPerProject = byStatusPerProject;
            ViewBag.NewCustomersPerMonth = newCustomersPerMonth;
            ViewBag.ByCity = byCity;
            ViewBag.BySize = bySize;

            return View();
        }

        /// <summary>Customer Blocking form: change customer status (block/unblock) with required reason and optional attachment. Logs to BlockingLogs and ActivityLog.</summary>
        [HttpGet]
        public async Task<IActionResult> CustomerBlocking()
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;

            var statusConfig = _context.Configurations
                .FirstOrDefault(c => c.ConfigKey == "customerstatus");
            ViewBag.CustomerStatuses = statusConfig != null
                ? statusConfig.ConfigValue.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).ToList()
                : new List<string> { "Active", "Blocked", "Inactive" };

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SearchCustomerForBlocking(string customerId)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                return Json(new { success = false, message = "Enter a Customer ID." });

            var customer = await _context.Customers
                .Where(c => c.CustomerID == customerId.Trim())
                .Select(c => new { c.CustomerID, c.FullName, c.Status, c.Phone, c.CNIC })
                .FirstOrDefaultAsync();

            if (customer == null)
                return Json(new { success = false, message = $"No customer found with ID '{customerId.Trim()}'." });

            return Json(new
            {
                success = true,
                customerID = customer.CustomerID,
                fullName = customer.FullName ?? "",
                status = customer.Status ?? "N/A",
                phone = customer.Phone ?? "",
                cnic = customer.CNIC ?? ""
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CustomerBlocking(string customerId, string newStatus, string reason, IFormFile? attachment)
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
            if (string.IsNullOrWhiteSpace(customerId))
            {
                TempData["ErrorMessage"] = "Please select a customer.";
                return RedirectToAction(nameof(CustomerBlocking));
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["ErrorMessage"] = "Reason is required.";
                return RedirectToAction(nameof(CustomerBlocking));
            }

            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Customer not found.";
                return RedirectToAction(nameof(CustomerBlocking));
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "User not identified.";
                return RedirectToAction(nameof(CustomerBlocking));
            }

            var previousStatus = customer.Status ?? "Unknown";
            customer.Status = newStatus?.Trim() ?? previousStatus;
            _context.Customers.Update(customer);

            string? attachmentPath = null;
            if (attachment != null && attachment.Length > 0)
            {
                var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
                var maxSize = 5 * 1024 * 1024; // 5MB
                var ext = Path.GetExtension(attachment.FileName)?.ToLowerInvariant();
                if (string.IsNullOrEmpty(ext) || !allowedExtensions.Contains(ext))
                {
                    TempData["ErrorMessage"] = "Invalid attachment type. Allowed: PDF, JPG, PNG, DOC, DOCX.";
                    return RedirectToAction(nameof(CustomerBlocking));
                }
                if (attachment.Length > maxSize)
                {
                    TempData["ErrorMessage"] = "Attachment must be 5MB or less.";
                    return RedirectToAction(nameof(CustomerBlocking));
                }
                var blockingDir = Path.Combine("wwwroot", "uploads", "blocking");
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), blockingDir));
                var fileName = $"{customerId}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), blockingDir, fileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                    await attachment.CopyToAsync(stream);
                attachmentPath = Path.Combine("uploads", "blocking", fileName).Replace('\\', '/');
            }

            var blockingLog = new BlockingLog
            {
                CustomerID = customerId,
                UserID = userId,
                ActionDate = DateTime.Now,
                PreviousStatus = previousStatus,
                NewStatus = customer.Status,
                Reason = reason.Trim(),
                AttachmentPath = attachmentPath
            };
            _context.BlockingLogs.Add(blockingLog);
            await _context.SaveChangesAsync();

            var actionLabel = (customer.Status ?? "").Equals("Blocked", StringComparison.OrdinalIgnoreCase) ? "Customer Blocked" : "Customer Unblocked";
            await LogActivity(userId, $"{actionLabel} - ID: {customerId} (Reason in BlockingLogs)", "Customer", customerId);

            TempData["SuccessMessage"] = $"Customer status updated to {customer.Status}. Logged in BlockingLogs and ActivityLog.";
            return RedirectToAction(nameof(CustomerBlocking));
        }

        [HttpPost]
        public async Task<IActionResult> GetProjectCustomers(string projectId, string size, int page = 1, int pageSize = 20)
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;
            try
            {
                var customers = await _context.Customers
                    .Include(c => c.Registration)
                    .Include(c => c.PaymentPlan)
                        .ThenInclude(p => p.Project)
                    .Where(c => c.PaymentPlan != null && 
                               c.PaymentPlan.ProjectID == projectId &&
                               (string.IsNullOrEmpty(size) || c.RegisteredSize == size))
                    .OrderBy(c => c.FullName)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new {
                        customerID = c.CustomerID,
                        fullName = c.FullName,
                        fatherName = c.FatherName,
                        phone = c.Phone,
                        email = c.Email,
                        city = c.City,
                        status = c.Status,
                        registeredSize = c.RegisteredSize,
                        createdAt = c.CreatedAt.ToString("MMM dd, yyyy")
                    })
                    .ToListAsync();

                var totalCount = await _context.Customers
                    .Include(c => c.PaymentPlan)
                    .Where(c => c.PaymentPlan != null && 
                               c.PaymentPlan.ProjectID == projectId &&
                               (string.IsNullOrEmpty(size) || c.RegisteredSize == size))
                    .CountAsync();

                var result = new {
                    customers,
                    totalCount,
                    currentPage = page,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    hasNextPage = page * pageSize < totalCount,
                    hasPrevPage = page > 1
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetProjectSizeGroups(string projectId)
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;
            try
            {
                var sizeGroups = await _context.Customers
                    .Include(c => c.PaymentPlan)
                    .Where(c => c.PaymentPlan != null && c.PaymentPlan.ProjectID == projectId)
                    .GroupBy(c => c.RegisteredSize ?? "Unknown")
                    .Select(g => new {
                        size = g.Key,
                        count = g.Count()
                    })
                    .OrderBy(g => g.size)
                    .ToListAsync();

                return Json(new {
                    sizeGroups
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // POST: Get Available Properties for Customer (matching project and size)
        [HttpPost]
        public async Task<IActionResult> GetAvailablePropertiesForCustomer(string customerID, string? projectID = null, string? registeredSize = null, int? dealerID = null)
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;
            try
            {
                var customer = await _context.Customers
                    .Include(c => c.Project)
                    .Include(c => c.Allotments)
                    .FirstOrDefaultAsync(c => c.CustomerID == customerID);

                if (customer == null)
                {
                    return Json(new { success = false, message = "Customer not found" });
                }

                var projectIdToUse = !string.IsNullOrWhiteSpace(projectID) ? projectID : customer.ProjectID;
                var registeredSizeToUse = !string.IsNullOrWhiteSpace(registeredSize) ? registeredSize : customer.RegisteredSize;
                var dealerIdToUse = dealerID ?? customer.DealerID;

                if (string.IsNullOrEmpty(projectIdToUse))
                {
                    return Json(new { success = false, message = "Customer does not have a project assigned" });
                }

                if (string.IsNullOrEmpty(registeredSizeToUse))
                {
                    return Json(new { success = false, message = "Customer does not have a registered size" });
                }

                // Resolve project for display
                var projectEntity = customer.Project;
                if ((projectEntity == null || projectEntity.ProjectID != projectIdToUse) && !string.IsNullOrEmpty(projectIdToUse))
                {
                    projectEntity = await _context.Projects.FirstOrDefaultAsync(p => p.ProjectID == projectIdToUse);
                }

                var currentPropertyId = customer.Allotments?
                    .OrderByDescending(a => a.AllotmentDate)
                    .Select(a => a.PropertyID)
                    .FirstOrDefault();

                var availableProperties = await _context.Properties
                    .Where(p => p.Status == "Available"
                        && p.ProjectID == projectIdToUse
                        && p.Size == registeredSizeToUse
                        && (!dealerIdToUse.HasValue ? true : p.DealerID == dealerIdToUse))
                    .OrderBy(p => p.PlotNo)
                    .Select(p => new
                    {
                        propertyID = p.PropertyID,
                        plotNo = p.PlotNo,
                        block = p.Block,
                        size = p.Size,
                        propertyType = p.PropertyType,
                        street = p.Street,
                        plotType = p.PlotType,
                        isCurrent = false
                    })
                    .ToListAsync();

                if (!string.IsNullOrEmpty(currentPropertyId))
                {
                    var currentProperty = await _context.Properties
                        .Where(p => p.PropertyID == currentPropertyId)
                        .Select(p => new
                        {
                            propertyID = p.PropertyID,
                            plotNo = p.PlotNo,
                            block = p.Block,
                            propertySize = p.Size,
                            propertyType = p.PropertyType,
                            street = p.Street,
                            plotType = p.PlotType,
                            projectID = p.ProjectID,
                            DealerID = p.DealerID
                        })
                        .FirstOrDefaultAsync();

                    if (currentProperty != null)
                    {
                        var matchesFilters = currentProperty.projectID == projectIdToUse &&
                                             currentProperty.propertySize == registeredSizeToUse &&
                                             (!dealerIdToUse.HasValue ? true : currentProperty.DealerID == dealerIdToUse);

                        if (matchesFilters && !availableProperties.Any(p => p.propertyID == currentProperty.propertyID))
                        {
                            availableProperties.Insert(0, new
                            {
                                propertyID = currentProperty.propertyID,
                                plotNo = currentProperty.plotNo,
                                block = currentProperty.block,
                                size = currentProperty.propertySize,
                                propertyType = currentProperty.propertyType,
                                street = currentProperty.street,
                                plotType = currentProperty.plotType,
                                isCurrent = true
                            });
                        }
                    }
                }

                return Json(new
                {
                    success = true,
                    properties = availableProperties,
                    projectName = projectEntity?.ProjectName ?? customer.Project?.ProjectName,
                    registeredSize = registeredSizeToUse
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailablePropertiesByProject(string projectId, string registeredSize, int? dealerId)
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;
            try
            {
                if (string.IsNullOrWhiteSpace(projectId) || string.IsNullOrWhiteSpace(registeredSize))
                {
                    return Json(new { success = false, message = "Project and registered size are required." });
                }

                var properties = await _context.Properties
                    .Where(p => p.Status == "Available"
                        && p.ProjectID == projectId
                        && p.Size == registeredSize
                        && (!dealerId.HasValue ? true : p.DealerID == dealerId))
                    .OrderBy(p => p.PlotNo)
                    .Select(p => new
                    {
                        propertyID = p.PropertyID,
                        plotNo = p.PlotNo,
                        block = p.Block,
                        size = p.Size,
                        propertyType = p.PropertyType,
                        street = p.Street,
                        plotType = p.PlotType
                    })
                    .ToListAsync();

                var project = await _context.Projects.FirstOrDefaultAsync(p => p.ProjectID == projectId);

                return Json(new
                {
                    success = true,
                    properties,
                    projectName = project?.ProjectName,
                    registeredSize
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;
            if (id == null)
            {
                return NotFound();
            }

            var customerIdTrimmed = id?.Trim();
            var customer = await _context.Customers
                .Include(c => c.Registration)
                .Include(c => c.PaymentPlan)
                    .ThenInclude(pp => pp.PaymentSchedules)
                        .ThenInclude(ps => ps.Payments)
                .Include(c => c.CustomerLogs)
                .Include(c => c.Allotments)
                    .ThenInclude(a => a.Property)
                .Include(c => c.Transfers)
                .Include(c => c.Dealer)
                .FirstOrDefaultAsync(c => c.CustomerID == id);

            if (customer == null)
            {
                return NotFound();
            }

            var activityLogs = await _context.ActivityLogs
                .AsNoTracking()
                .Include(a => a.User)
                .Where(a => a.RefID != null && a.RefID.Trim() == (customer.CustomerID ?? "").Trim())
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
            ViewBag.ActivityLogs = activityLogs;

            return View(customer);
        }

        [HttpGet]
        public async Task<IActionResult> AccountStatement(string id)
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .Include(c => c.PaymentPlan)
                    .ThenInclude(pp => pp.PaymentSchedules)
                        .ThenInclude(ps => ps.Payments)
                .Include(c => c.Allotments)
                    .ThenInclude(a => a.Property)
                .FirstOrDefaultAsync(c => c.CustomerID == id);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        /// <summary>
        /// Printable allotment letter for customers who have been allotted a property. Proof of allotment with customer and property details.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AllotmentLetter(string id)
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var customer = await _context.Customers
                .Include(c => c.PaymentPlan)
                    .ThenInclude(pp => pp!.Project)
                .Include(c => c.Allotments)
                    .ThenInclude(a => a!.Property)
                        .ThenInclude(p => p!.Project)
                .FirstOrDefaultAsync(c => c.CustomerID == id);

            if (customer == null)
                return NotFound();

            if (customer.Allotments == null || !customer.Allotments.Any())
            {
                TempData["ErrorMessage"] = "Allotment letter is only available for customers who have been allotted a property.";
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(customer);
        }

        // POST: Search Registration by RegID (AJAX)
        [HttpPost]
        public async Task<IActionResult> SearchRegistration(string regID)
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;
            if (string.IsNullOrEmpty(regID))
            {
                return Json(new { success = false, message = "Please enter a Registration ID" });
            }

            var registration = await _context.Registrations
                .Include(r => r.Customers)
                .FirstOrDefaultAsync(r => r.RegID == regID);

            if (registration == null)
            {
                return Json(new { success = false, message = "Registration not found with ID: " + regID });
            }

            // Check if registration is already linked to a customer
            if (registration.Customers != null && registration.Customers.Any())
            {
                var existingCustomer = registration.Customers.FirstOrDefault();
                return Json(new { 
                    success = false, 
                    message = $"This registration is already linked to Customer: {existingCustomer?.FullName} ({existingCustomer?.CustomerID})" 
                });
            }

            // Return registration data
            return Json(new
            {
                success = true,
                registration = new
                {
                    regID = registration.RegID,
                    fullName = registration.FullName,
                    cnic = registration.CNIC,
                    phone = registration.Phone,
                    email = registration.Email,
                    status = registration.Status
                }
            });
        }

        public async Task<IActionResult> Create()
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
            // Load registrations for dropdown (also supports AJAX search)
            ViewBag.Registrations = _context.Registrations.ToList();
            ViewBag.Projects = _context.Projects
                .OrderBy(p => p.ProjectName)
                .ToList();
            ViewBag.PaymentPlans = _context.PaymentPlans
                .Include(pp => pp.Project)
                .ToList();
            ViewBag.Dealers = _context.Dealers.Where(d => d.Status == "Active").ToList();
            
            // Load configurations (comma-separated values)
            var citiesConfig = _context.Configurations
                .FirstOrDefault(c => c.ConfigKey == "cities");
            ViewBag.Cities = citiesConfig != null 
                ? citiesConfig.ConfigValue.Split(',').Select(s => s.Trim()).ToList()
                : new List<string>();
            
            var countriesConfig = _context.Configurations
                .FirstOrDefault(c => c.ConfigKey == "countries");
            ViewBag.Countries = countriesConfig != null 
                ? countriesConfig.ConfigValue.Split(',').Select(s => s.Trim()).ToList()
                : new List<string>();
            
            var sizesConfig = _context.Configurations
                .FirstOrDefault(c => c.ConfigKey == "sizes");
            ViewBag.Sizes = sizesConfig != null 
                ? sizesConfig.ConfigValue.Split(',').Select(s => s.Trim()).ToList()
                : new List<string>();
            
            var subProjectsConfig = _context.Configurations
                .FirstOrDefault(c => c.ConfigKey == "subprojects");
            ViewBag.SubProjects = subProjectsConfig != null 
                ? subProjectsConfig.ConfigValue.Split(',').Select(s => s.Trim()).ToList()
                : new List<string>();

            var nationalitiesConfig = _context.Configurations
                .FirstOrDefault(c => c.ConfigKey == "nationalities");
            ViewBag.Nationalities = nationalitiesConfig != null
                ? nationalitiesConfig.ConfigValue.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList()
                : new List<string> { "Pakistani", "American", "British", "Canadian", "Chinese", "Indian", "Other" };

            ViewBag.SelectedPropertyID = null;
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer, IFormFile? nomineeNICUpload, IFormFile? nomineePictureUpload, string? selectedPropertyID)
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
            var nomineeNicValidationError = ValidateKinFile(nomineeNICUpload);
            if (nomineeNicValidationError != null)
            {
                ModelState.AddModelError(nameof(customer.NomineeNICDocumentPath), nomineeNicValidationError);
            }

            var nomineePictureValidationError = ValidateKinFile(nomineePictureUpload);
            if (nomineePictureValidationError != null)
            {
                ModelState.AddModelError(nameof(customer.NomineePicturePath), nomineePictureValidationError);
            }

            if (customer.IsDealerRegistered == 0)
            {
                customer.DealerID = null;
            }
            else if (customer.IsDealerRegistered == 1)
            {
                customer.DealerName = null;
            }

            if (ModelState.IsValid)
            {
                // Generate CustomerID based on Project Prefix
                customer.CustomerID = await GenerateCustomerID(customer.ProjectID);
                customer.CreatedAt = DateTime.Now;
                customer.Status = "Active";

                if (nomineeNICUpload != null && nomineeNICUpload.Length > 0)
                {
                    customer.NomineeNICDocumentPath = await SaveKinFileAsync(customer.CustomerID, nomineeNICUpload, "kin-nic");
                }

                if (nomineePictureUpload != null && nomineePictureUpload.Length > 0)
                {
                    customer.NomineePicturePath = await SaveKinFileAsync(customer.CustomerID, nomineePictureUpload, "kin-picture");
                }

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    var actionDetail = $"Customer Creation - {customer.FullName ?? "N/A"} (CNIC: {customer.CNIC ?? "N/A"})";
                    await LogActivity(userId, actionDetail, "Customer", customer.CustomerID);
                }

                if (!string.IsNullOrWhiteSpace(selectedPropertyID))
                {
                    try
                    {
                        await AssignCustomerPropertyAsync(customer.CustomerID, selectedPropertyID);
                    }
                    catch (InvalidOperationException ex)
                    {
                        TempData["ErrorMessage"] = ex.Message;
                    }
                }

                // Redirect to Edit page so user can upload attachments
                return RedirectToAction(nameof(Edit), new { id = customer.CustomerID });
            }

            // Reload data on validation error
            ViewBag.Registrations = _context.Registrations.ToList();
            ViewBag.Projects = _context.Projects
                .OrderBy(p => p.ProjectName)
                .ToList();
            ViewBag.PaymentPlans = _context.PaymentPlans
                .Include(pp => pp.Project)
                .ToList();
            ViewBag.Dealers = _context.Dealers.Where(d => d.Status == "Active").ToList();
            
            // Reload configurations (comma-separated values)
            var citiesConfig = _context.Configurations
                .FirstOrDefault(c => c.ConfigKey == "cities");
            ViewBag.Cities = citiesConfig != null 
                ? citiesConfig.ConfigValue.Split(',').Select(s => s.Trim()).ToList()
                : new List<string>();
            
            var countriesConfig = _context.Configurations
                .FirstOrDefault(c => c.ConfigKey == "countries");
            ViewBag.Countries = countriesConfig != null 
                ? countriesConfig.ConfigValue.Split(',').Select(s => s.Trim()).ToList()
                : new List<string>();
            
            var sizesConfig = _context.Configurations
                .FirstOrDefault(c => c.ConfigKey == "sizes");
            ViewBag.Sizes = sizesConfig != null 
                ? sizesConfig.ConfigValue.Split(',').Select(s => s.Trim()).ToList()
                : new List<string>();
            
            var subProjectsConfig = _context.Configurations
                .FirstOrDefault(c => c.ConfigKey == "subprojects");
            ViewBag.SubProjects = subProjectsConfig != null 
                ? subProjectsConfig.ConfigValue.Split(',').Select(s => s.Trim()).ToList()
                : new List<string>();

            var nationalitiesConfig = _context.Configurations
                .FirstOrDefault(c => c.ConfigKey == "nationalities");
            ViewBag.Nationalities = nationalitiesConfig != null
                ? nationalitiesConfig.ConfigValue.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList()
                : new List<string> { "Pakistani", "American", "British", "Canadian", "Chinese", "Indian", "Other" };

            ViewBag.SelectedPropertyID = selectedPropertyID;
            
            return View(customer);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
            id = NormalizeId(id);
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .Include(c => c.Allotments)
                    .ThenInclude(a => a.Property)
                .Include(c => c.Project)
                .Include(c => c.PaymentPlan)
                    .ThenInclude(p => p.Project)
                .FirstOrDefaultAsync(c => c.CustomerID == id);

            if (customer == null)
            {
                return NotFound();
            }

            if (!customer.IsDealerRegistered.HasValue)
            {
                customer.IsDealerRegistered = !string.IsNullOrWhiteSpace(customer.DealerName) ? 0 : 1;
            }

            ViewBag.Registrations = _context.Registrations.ToList();
            ViewBag.Projects = _context.Projects
                .OrderBy(p => p.ProjectName)
                .ToList();
            ViewBag.PaymentPlans = _context.PaymentPlans
                .Include(pp => pp.Project)
                .ToList();
            ViewBag.Dealers = _context.Dealers.Where(d => d.Status == "Active").ToList();
            
            // Load configurations (comma-separated values)
            var citiesConfig = _context.Configurations
                .FirstOrDefault(c => c.ConfigKey == "cities");
            ViewBag.Cities = citiesConfig != null 
                ? citiesConfig.ConfigValue.Split(',').Select(s => s.Trim()).ToList()
                : new List<string>();
            
            var countriesConfig = _context.Configurations
                .FirstOrDefault(c => c.ConfigKey == "countries");
            ViewBag.Countries = countriesConfig != null 
                ? countriesConfig.ConfigValue.Split(',').Select(s => s.Trim()).ToList()
                : new List<string>();
            
            var sizesConfig = _context.Configurations
                .FirstOrDefault(c => c.ConfigKey == "sizes");
            ViewBag.Sizes = sizesConfig != null 
                ? sizesConfig.ConfigValue.Split(',').Select(s => s.Trim()).ToList()
                : new List<string>();
            
            var subProjectsConfig = _context.Configurations
                .FirstOrDefault(c => c.ConfigKey == "subprojects");
            ViewBag.SubProjects = subProjectsConfig != null 
                ? subProjectsConfig.ConfigValue.Split(',').Select(s => s.Trim()).ToList()
                : new List<string>();

            var nationalitiesConfig = _context.Configurations
                .FirstOrDefault(c => c.ConfigKey == "nationalities");
            ViewBag.Nationalities = nationalitiesConfig != null
                ? nationalitiesConfig.ConfigValue.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList()
                : new List<string> { "Pakistani", "American", "British", "Canadian", "Chinese", "Indian", "Other" };

            // Load allotment types
            var allotmentTypesConfig = _context.Configurations
                .FirstOrDefault(c => c.ConfigKey == "allotmenttypes");
            ViewBag.AllotmentTypes = allotmentTypesConfig != null 
                ? allotmentTypesConfig.ConfigValue.Split(',').Select(s => s.Trim()).ToList()
                : new List<string> { "Regular", "Transfer", "Balloting", "Special" };

            ViewBag.SelectedPropertyID = customer.Allotments?.FirstOrDefault()?.PropertyID;
            
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Customer customer, IFormFile? nomineeNICUpload, IFormFile? nomineePictureUpload, string? selectedPropertyID)
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
            id = NormalizeId(id);
            customer.CustomerID = NormalizeId(customer.CustomerID);
            if (string.IsNullOrEmpty(id) || id != customer.CustomerID)
            {
                return NotFound();
            }

            var nomineeNicValidationError = ValidateKinFile(nomineeNICUpload);
            if (nomineeNicValidationError != null)
            {
                ModelState.AddModelError(nameof(customer.NomineeNICDocumentPath), nomineeNicValidationError);
            }

            var nomineePictureValidationError = ValidateKinFile(nomineePictureUpload);
            if (nomineePictureValidationError != null)
            {
                ModelState.AddModelError(nameof(customer.NomineePicturePath), nomineePictureValidationError);
            }

            if (customer.IsDealerRegistered == 0)
            {
                customer.DealerID = null;
            }
            else if (customer.IsDealerRegistered == 1)
            {
                customer.DealerName = null;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCustomer = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.CustomerID == id);
                    if (existingCustomer == null)
                    {
                        return NotFound();
                    }

                    if (nomineeNICUpload != null && nomineeNICUpload.Length > 0)
                    {
                        DeleteKinFileIfExists(existingCustomer.NomineeNICDocumentPath);
                        customer.NomineeNICDocumentPath = await SaveKinFileAsync(customer.CustomerID, nomineeNICUpload, "kin-nic");
                    }
                    else
                    {
                        customer.NomineeNICDocumentPath = existingCustomer.NomineeNICDocumentPath;
                    }

                    if (nomineePictureUpload != null && nomineePictureUpload.Length > 0)
                    {
                        DeleteKinFileIfExists(existingCustomer.NomineePicturePath);
                        customer.NomineePicturePath = await SaveKinFileAsync(customer.CustomerID, nomineePictureUpload, "kin-picture");
                    }
                    else
                    {
                        customer.NomineePicturePath = existingCustomer.NomineePicturePath;
                    }

                    _context.Update(customer);
                    await _context.SaveChangesAsync();

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var actionDetail = $"Customer Updation - {customer.FullName ?? "N/A"} (CNIC: {customer.CNIC ?? "N/A"})";
                        await LogActivity(userId, actionDetail, "Customer", customer.CustomerID);
                    }

                    if (!string.IsNullOrWhiteSpace(selectedPropertyID))
                    {
                        try
                        {
                            await AssignCustomerPropertyAsync(customer.CustomerID, selectedPropertyID);
                        }
                        catch (InvalidOperationException ex)
                        {
                            TempData["ErrorMessage"] = ex.Message;
                        }
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.CustomerID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Registrations = _context.Registrations.ToList();
            ViewBag.Projects = _context.Projects
                .OrderBy(p => p.ProjectName)
                .ToList();
            ViewBag.PaymentPlans = _context.PaymentPlans
                .Include(pp => pp.Project)
                .ToList();
            ViewBag.Dealers = _context.Dealers.Where(d => d.Status == "Active").ToList();
            
            // Reload configurations (comma-separated values)
            var citiesConfig = _context.Configurations
                .FirstOrDefault(c => c.ConfigKey == "cities");
            ViewBag.Cities = citiesConfig != null 
                ? citiesConfig.ConfigValue.Split(',').Select(s => s.Trim()).ToList()
                : new List<string>();
            
            var countriesConfig = _context.Configurations
                .FirstOrDefault(c => c.ConfigKey == "countries");
            ViewBag.Countries = countriesConfig != null 
                ? countriesConfig.ConfigValue.Split(',').Select(s => s.Trim()).ToList()
                : new List<string>();
            
            var sizesConfig = _context.Configurations
                .FirstOrDefault(c => c.ConfigKey == "sizes");
            ViewBag.Sizes = sizesConfig != null 
                ? sizesConfig.ConfigValue.Split(',').Select(s => s.Trim()).ToList()
                : new List<string>();
            
            var subProjectsConfig = _context.Configurations
                .FirstOrDefault(c => c.ConfigKey == "subprojects");
            ViewBag.SubProjects = subProjectsConfig != null 
                ? subProjectsConfig.ConfigValue.Split(',').Select(s => s.Trim()).ToList()
                : new List<string>();

            var nationalitiesConfig = _context.Configurations
                .FirstOrDefault(c => c.ConfigKey == "nationalities");
            ViewBag.Nationalities = nationalitiesConfig != null
                ? nationalitiesConfig.ConfigValue.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList()
                : new List<string> { "Pakistani", "American", "British", "Canadian", "Chinese", "Indian", "Other" };

            ViewBag.SelectedPropertyID = selectedPropertyID;
            
            return View(customer);
        }

        public async Task<IActionResult> Delete(string id)
        {
            var denied = await EnsurePermissionAsync("Admin");
            if (denied != null) return denied;
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .Include(c => c.Registration)
                .Include(c => c.PaymentPlan)
                .FirstOrDefaultAsync(c => c.CustomerID == id);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var denied = await EnsurePermissionAsync("Admin");
            if (denied != null) return denied;
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                var fullName = customer.FullName ?? "N/A";
                var cnic = customer.CNIC ?? "N/A";
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    var actionDetail = $"Customer Deletion - {fullName} (CNIC: {cnic})";
                    await LogActivity(userId, actionDetail, "Customer", id);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(string? id)
        {
            return !string.IsNullOrEmpty(id) && _context.Customers.Any(e => e.CustomerID == id);
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

        // Generate CustomerID based on Project Prefix: "JSC0001", "JSC0002", etc. (no dash)
        private async Task<string> GenerateCustomerID(string? projectID)
        {
            if (string.IsNullOrEmpty(projectID))
            {
                // Fallback to random if no project selected
                return GenerateID();
            }

            // Get Project directly
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectID == projectID);

            if (project == null || string.IsNullOrEmpty(project.Prefix))
            {
                // Fallback to random if project or prefix not found
                return GenerateID();
            }

            string projectPrefix = project.Prefix;

            // Get all existing customers with this project prefix (check both with and without dash for backward compatibility)
            var existingCustomers = await _context.Customers
                .Where(c => c.ProjectID == projectID && c.CustomerID != null &&
                           (c.CustomerID.StartsWith(projectPrefix) || c.CustomerID.StartsWith(projectPrefix + "-")))
                .Select(c => c.CustomerID)
                .ToListAsync();

            // Extract numeric part from existing CustomerIDs (format: "JSC0001" or "JSC-00001" for backward compatibility)
            int maxNumber = 0;
            foreach (var existingCustomerID in existingCustomers)
            {
                if (string.IsNullOrEmpty(existingCustomerID)) continue;
                // Remove prefix to get the number part
                if (existingCustomerID.StartsWith(projectPrefix))
                {
                    string existingNumberPart = existingCustomerID.Substring(projectPrefix.Length);
                    
                    // Remove dash if present (for backward compatibility)
                    if (existingNumberPart.StartsWith("-"))
                    {
                        existingNumberPart = existingNumberPart.Substring(1);
                    }
                    
                    // Try to parse the number part
                    if (int.TryParse(existingNumberPart, out int parsedNumber))
                    {
                        if (parsedNumber > maxNumber)
                        {
                            maxNumber = parsedNumber;
                        }
                    }
                }
            }

            // Generate next sequential number
            int nextNumber = maxNumber + 1;
            
            // Calculate available length for number part (10 total - prefix length, no dash)
            int availableLength = 10 - projectPrefix.Length;
            
            if (availableLength <= 0)
            {
                // Prefix is too long, fallback to random
                return GenerateID();
            }
            
            // Format number with appropriate padding (max 5 digits, but adjust if prefix is longer)
            int maxDigits = Math.Min(5, availableLength);
            string numberPart = nextNumber.ToString().PadLeft(maxDigits, '0');
            
            // If number exceeds available length, truncate
            if (numberPart.Length > availableLength)
            {
                numberPart = numberPart.Substring(numberPart.Length - availableLength);
            }
            
            // Format: Prefix + Number (no dash)
            string customerID = $"{projectPrefix}{numberPart}";

            return customerID;
        }

        private string? ValidateKinFile(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedKinFileExtensions.Contains(extension))
            {
                return "Only image (JPG, JPEG, PNG, GIF, BMP) and PDF files are allowed for kin documents.";
            }

            if (file.Length > _maxKinFileSize)
            {
                return "Kin document file size cannot exceed 8MB.";
            }

            return null;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GenerateTestCustomers(int customersPerProject = 100)
        {
            var denied = await EnsurePermissionAsync("Admin");
            if (denied != null) return denied;
            try
            {
                // Get all projects
                var projects = await _context.Projects
                    .Where(p => !string.IsNullOrEmpty(p.Prefix))
                    .ToListAsync();

                if (!projects.Any())
                {
                    TempData["Error"] = "No projects found with prefixes.";
                    return RedirectToAction(nameof(Index));
                }

                // Get sizes and subprojects from Configuration
                var sizesConfig = await _context.Configurations
                    .FirstOrDefaultAsync(c => c.ConfigKey == "sizes");
                var sizes = sizesConfig?.ConfigValue?.Split(',').Select(s => s.Trim()).ToArray()
                    ?? new[] { "5 Marla", "7 Marla", "10 Marla" };

                var subProjectsConfig = await _context.Configurations
                    .FirstOrDefaultAsync(c => c.ConfigKey == "subprojects");
                var subProjects = subProjectsConfig?.ConfigValue?.Split(',').Select(s => s.Trim()).ToArray()
                    ?? new[] { "Phase 1", "Phase 2", "Phase 3" };

                // Get payment plans grouped by project
                var paymentPlansByProject = await _context.PaymentPlans
                    .Where(pp => pp.ProjectID != null)
                    .GroupBy(pp => pp.ProjectID!)
                    .ToDictionaryAsync(g => g.Key, g => g.ToList());

                var random = new Random();
                var firstNames = new[] { "Ahmed", "Ali", "Hassan", "Hussain", "Muhammad", "Usman", "Bilal", "Zain", "Hamza", "Omar", "Fatima", "Ayesha", "Zainab", "Maryam", "Khadija" };
                var lastNames = new[] { "Khan", "Ahmed", "Ali", "Hassan", "Hussain", "Malik", "Sheikh", "Butt", "Raza", "Iqbal" };
                var cities = new[] { "Karachi", "Lahore", "Islamabad", "Rawalpindi", "Faisalabad" };
                var genders = new[] { "Male", "Female" };

                int totalCreated = 0;

                foreach (var project in projects)
                {
                    if (!paymentPlansByProject.TryGetValue(project.ProjectID, out var paymentPlans) || !paymentPlans.Any())
                    {
                        continue;
                    }

                    // Get existing customers to find max number
                    var existingCustomers = await _context.Customers
                        .Where(c => c.ProjectID == project.ProjectID &&
                                   (c.CustomerID.StartsWith(project.Prefix) || c.CustomerID.StartsWith(project.Prefix + "-")))
                        .Select(c => c.CustomerID)
                        .ToListAsync();

                    int maxNumber = 0;
                    foreach (var existingCustomerID in existingCustomers)
                    {
                        if (existingCustomerID.StartsWith(project.Prefix))
                        {
                            string existingNumberPart = existingCustomerID.Substring(project.Prefix.Length);
                            if (existingNumberPart.StartsWith("-"))
                                existingNumberPart = existingNumberPart.Substring(1);
                            if (int.TryParse(existingNumberPart, out int parsedNumber) && parsedNumber > maxNumber)
                                maxNumber = parsedNumber;
                        }
                    }

                    int startNumber = maxNumber + 1;
                    int createdForProject = 0;

                    for (int i = 0; i < customersPerProject; i++)
                    {
                        try
                        {
                            int customerNumber = startNumber + i;
                            int availableLength = 10 - project.Prefix.Length;
                            int maxDigits = Math.Min(5, availableLength);
                            string numberPart = customerNumber.ToString().PadLeft(maxDigits, '0');
                            if (numberPart.Length > availableLength)
                                numberPart = numberPart.Substring(numberPart.Length - availableLength);
                            string customerID = $"{project.Prefix}{numberPart}";

                            if (await _context.Customers.AnyAsync(c => c.CustomerID == customerID))
                                continue;

                            var selectedPlan = paymentPlans[random.Next(paymentPlans.Count)];

                            var customer = new Customer
                            {
                                CustomerID = customerID,
                                ProjectID = project.ProjectID,
                                PlanID = selectedPlan.PlanID,
                                FullName = $"{firstNames[random.Next(firstNames.Length)]} {lastNames[random.Next(lastNames.Length)]}",
                                FatherName = $"{firstNames[random.Next(firstNames.Length)]} {lastNames[random.Next(lastNames.Length)]}",
                                CNIC = $"{random.Next(10000, 99999)}-{random.Next(1000000, 9999999)}-{random.Next(1, 9)}",
                                Phone = $"03{random.Next(10, 99)}-{random.Next(1000000, 9999999)}",
                                Email = $"customer{customerID.ToLower()}@example.com",
                                Gender = genders[random.Next(genders.Length)],
                                Nationality = "Pakistani",
                                City = cities[random.Next(cities.Length)],
                                Country = "Pakistan",
                                MailingAddress = $"House #{random.Next(1, 999)}, Street {random.Next(1, 50)}, {cities[random.Next(cities.Length)]}",
                                PermanentAddress = $"House #{random.Next(1, 999)}, Street {random.Next(1, 50)}, {cities[random.Next(cities.Length)]}",
                                SubProject = subProjects[random.Next(subProjects.Length)],
                                RegisteredSize = sizes[random.Next(sizes.Length)],
                                Status = "Active",
                                CreatedAt = DateTime.Now.AddDays(-random.Next(365))
                            };

                            _context.Customers.Add(customer);
                            createdForProject++;

                            if (createdForProject % 50 == 0)
                            {
                                await _context.SaveChangesAsync();
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log error but continue
                            System.Diagnostics.Debug.WriteLine($"Error creating customer: {ex.Message}");
                        }
                    }

                    if (createdForProject % 50 != 0)
                    {
                        await _context.SaveChangesAsync();
                    }

                    totalCreated += createdForProject;
                }

                TempData["Success"] = $"Successfully created {totalCreated} test customers.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error generating customers: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
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

        private async Task<string> SaveKinFileAsync(string customerId, IFormFile file, string filePrefix)
        {
            var safeCustomerSegment = SanitizePathSegment(customerId);
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "customers", safeCustomerSegment, "kin");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{filePrefix}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/customers/{safeCustomerSegment}/kin/{fileName}";
        }

        private void DeleteKinFileIfExists(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return;
            }

            var trimmedPath = relativePath.TrimStart('~').TrimStart('/');
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", trimmedPath.Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }

        private async Task<string> GenerateAllotmentIdAsync()
        {
            var existingIds = await _context.Allotments
                .Select(a => a.AllotmentID)
                .ToListAsync();

            int maxNumber = 0;
            foreach (var id in existingIds)
            {
                if (string.IsNullOrEmpty(id)) continue;
                if (int.TryParse(id, out var numericId))
                {
                    if (numericId > maxNumber)
                    {
                        maxNumber = numericId;
                    }
                }
                else if (id.StartsWith("ALLOT", StringComparison.OrdinalIgnoreCase) &&
                         int.TryParse(id.Substring(5), out var prefixedNumericId) &&
                         prefixedNumericId > maxNumber)
                {
                    maxNumber = prefixedNumericId;
                }
            }

            return (maxNumber + 1).ToString();
        }

        private async Task AssignCustomerPropertyAsync(string customerId, string? propertyId)
        {
            if (string.IsNullOrWhiteSpace(customerId) || string.IsNullOrWhiteSpace(propertyId))
            {
                return;
            }

            var customer = await _context.Customers
                .Include(c => c.Allotments)
                .FirstOrDefaultAsync(c => c.CustomerID == customerId);

            if (customer == null)
            {
                throw new InvalidOperationException("Customer could not be found for property assignment.");
            }

            var selectedProperty = await _context.Properties
                .Include(p => p.Allotments)
                .FirstOrDefaultAsync(p => p.PropertyID == propertyId);

            if (selectedProperty == null)
            {
                throw new InvalidOperationException("Selected property could not be found.");
            }

            var existingAllotment = customer.Allotments?.FirstOrDefault();

            if (existingAllotment != null && existingAllotment.PropertyID == propertyId)
            {
                // Nothing to change
                return;
            }

            if (selectedProperty.Allotments != null && selectedProperty.Allotments.Any(a => a.CustomerID != customerId))
            {
                throw new InvalidOperationException("Selected property is already allotted to another customer.");
            }

            if (!string.Equals(selectedProperty.Status, "Available", StringComparison.OrdinalIgnoreCase) &&
                (selectedProperty.Allotments == null || !selectedProperty.Allotments.Any()))
            {
                throw new InvalidOperationException("Selected property is not available for allotment.");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "SYSTEM";

            if (existingAllotment != null)
            {
                if (!string.IsNullOrEmpty(existingAllotment.PropertyID) && existingAllotment.PropertyID != propertyId)
                {
                    var previousProperty = await _context.Properties
                        .FirstOrDefaultAsync(p => p.PropertyID == existingAllotment.PropertyID);
                    if (previousProperty != null)
                    {
                        previousProperty.Status = "Available";
                    }
                }

                existingAllotment.PropertyID = propertyId;
                existingAllotment.AllottmentType ??= "Regular";
                existingAllotment.AllottedBy = userId;
                existingAllotment.AllotmentDate = DateTime.Now;
                existingAllotment.WorkFlowStatus = "Pending";
            }
            else
            {
                var allotment = new Allotment
                {
                    AllotmentID = await GenerateAllotmentIdAsync(),
                    CustomerID = customerId,
                    PropertyID = propertyId,
                    AllottedBy = userId,
                    AllotmentDate = DateTime.Now,
                    AllottmentType = "Regular",
                    WorkFlowStatus = "Pending"
                };

                _context.Allotments.Add(allotment);
            }

            selectedProperty.Status = "Allotted";
            await _context.SaveChangesAsync();
            await LogActivity(userId, $"Assigned property {propertyId} to customer {customerId}", "Allotment", propertyId);
        }

        // ===========================================================
        // ATTACHMENT MANAGEMENT
        // ===========================================================

        [HttpPost]
        public async Task<IActionResult> UploadAttachment(string customerId, IFormFile file, string attachmentType, string description = "")
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
            try
            {
                var normalizedCustomerId = NormalizeId(customerId);
                if (string.IsNullOrEmpty(normalizedCustomerId))
                {
                    return Json(new { success = false, message = "Customer ID is required" });
                }

                if (file == null || file.Length == 0)
                {
                    return Json(new { success = false, message = "Please select a file to upload" });
                }

                if (string.IsNullOrEmpty(attachmentType))
                {
                    return Json(new { success = false, message = "Attachment type is required" });
                }

                // Validate file type (images only)
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".pdf" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return Json(new { success = false, message = "Only image files (JPG, PNG, GIF, BMP) and PDF files are allowed" });
                }

                // Validate file size (max 8MB)
                const long maxFileSize = 8 * 1024 * 1024; // 8MB
                if (file.Length > maxFileSize)
                {
                    return Json(new { success = false, message = "File size exceeds 8MB limit" });
                }

                // Check if customer exists
                var customer = await _context.Customers.FindAsync(normalizedCustomerId);
                if (customer == null)
                {
                    return Json(new { success = false, message = "Customer not found" });
                }

                // Check for existing CustomerPicture or IDCard (only one allowed)
                if (attachmentType == "CustomerPicture" || attachmentType == "IDCard")
                {
                    var existing = await _context.Attachments
                        .FirstOrDefaultAsync(a => a.RefType == "Customer" && 
                                                  a.RefID == normalizedCustomerId && 
                                                  a.AttachmentType == attachmentType);
                    
                    if (existing != null)
                    {
                        // Delete existing file
                        if (!string.IsNullOrEmpty(existing.FilePath) && System.IO.File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existing.FilePath.TrimStart('/'))))
                        {
                            System.IO.File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existing.FilePath.TrimStart('/')));
                        }
                        _context.Attachments.Remove(existing);
                    }
                }

                // Create uploads directory if it doesn't exist
                var safeCustomerSegment = SanitizePathSegment(normalizedCustomerId);
                if (string.IsNullOrEmpty(safeCustomerSegment))
                {
                    return Json(new { success = false, message = "Invalid Customer ID for file storage" });
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "customers", safeCustomerSegment);
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate unique filename
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                var relativePath = $"/uploads/customers/{safeCustomerSegment}/{uniqueFileName}";

                // Save file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Generate attachment ID
                var attachmentID = GenerateID();

                // Save attachment record
                var attachment = new Attachment
                {
                    AttachmentID = attachmentID,
                    RefType = "Customer",
                    RefID = normalizedCustomerId,
                    AttachmentType = attachmentType,
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

                // Log activity
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    await LogActivity(userId, $"Upload {attachmentType}", "Attachment", attachmentID);
                }

                return Json(new { 
                    success = true, 
                    message = "File uploaded successfully",
                    attachment = new {
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
                return Json(new { success = false, message = $"Error uploading file: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAttachments(string customerId)
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;
            try
            {
                if (string.IsNullOrEmpty(customerId))
                {
                    return Json(new { success = false, message = "Customer ID is required" });
                }

                var attachments = await _context.Attachments
                    .Where(a => a.RefType == "Customer" && a.RefID == customerId)
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
                        uploadedAt = a.UploadedAt.ToString("MMM dd, yyyy hh:mm tt"),
                        uploadedBy = a.UploadedByUser != null ? a.UploadedByUser.FullName : "Unknown"
                    })
                    .ToListAsync();

                return Json(new { success = true, attachments });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error loading attachments: {ex.Message}" });
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
                {
                    return Json(new { success = false, message = "Attachment ID is required" });
                }

                var attachment = await _context.Attachments.FindAsync(attachmentId);
                if (attachment == null)
                {
                    return Json(new { success = false, message = "Attachment not found" });
                }

                // Delete physical file
                if (!string.IsNullOrEmpty(attachment.FilePath))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", attachment.FilePath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                // Delete record
                _context.Attachments.Remove(attachment);
                await _context.SaveChangesAsync();

                // Log activity
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    await LogActivity(userId, "Delete Attachment", "Attachment", attachmentId);
                }

                return Json(new { success = true, message = "Attachment deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error deleting attachment: {ex.Message}" });
            }
        }
    }
}
