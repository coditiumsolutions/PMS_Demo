using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS.Data;
using PMS.Models;
using System.Security.Claims;
using System.IO;

namespace PMS.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly PMSDbContext _context;

        private static readonly string[] _allowedKinFileExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".pdf" };
        private const long _maxKinFileSize = 8 * 1024 * 1024; // 8MB

        public CustomerController(PMSDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string projectFilter = "All", string statusFilter = "All", string searchTerm = "")
        {
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
                    c.CustomerID.ToLower().Contains(searchTerm) ||
                    c.FullName.ToLower().Contains(searchTerm) ||
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

        [HttpPost]
        public async Task<IActionResult> GetProjectCustomers(string projectId, string size, int page = 1, int pageSize = 20)
        {
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
        public async Task<IActionResult> GetAvailablePropertiesForCustomer(string customerID)
        {
            try
            {
                var customer = await _context.Customers
                    .Include(c => c.Project)
                    .FirstOrDefaultAsync(c => c.CustomerID == customerID);

                if (customer == null)
                {
                    return Json(new { success = false, message = "Customer not found" });
                }

                if (string.IsNullOrEmpty(customer.ProjectID))
                {
                    return Json(new { success = false, message = "Customer does not have a project assigned" });
                }

                if (string.IsNullOrEmpty(customer.RegisteredSize))
                {
                    return Json(new { success = false, message = "Customer does not have a registered size" });
                }

                var projectID = customer.ProjectID;
                var registeredSize = customer.RegisteredSize;

                // Get available properties matching customer's project and size
                var properties = await _context.Properties
                    .Include(p => p.Project)
                    .Where(p => p.Status == "Available" 
                        && p.ProjectID == projectID 
                        && p.Size == registeredSize)
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

                return Json(new { 
                    success = true, 
                    properties = properties,
                    projectName = customer.Project?.ProjectName,
                    registeredSize = registeredSize
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .Include(c => c.Registration)
                .Include(c => c.PaymentPlan)
                    .ThenInclude(pp => pp.PaymentSchedules)
                        .ThenInclude(ps => ps.Payments)
                .Include(c => c.CustomerLogs)
                .Include(c => c.Allotments)
                    .ThenInclude(a => a.Property)
                .FirstOrDefaultAsync(c => c.CustomerID == id);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        [HttpGet]
        public async Task<IActionResult> AccountStatement(string id)
        {
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

        // POST: Search Registration by RegID (AJAX)
        [HttpPost]
        public async Task<IActionResult> SearchRegistration(string regID)
        {
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

        public IActionResult Create()
        {
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
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer, IFormFile? nomineeNICUpload, IFormFile? nomineePictureUpload)
        {
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
                    await LogActivity(userId, "Create Customer", "Customer", customer.CustomerID);
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
            
            return View(customer);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
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
            
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Customer customer, IFormFile? nomineeNICUpload, IFormFile? nomineePictureUpload)
        {
            if (id != customer.CustomerID)
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
                        await LogActivity(userId, "Update Customer", "Customer", customer.CustomerID);
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
            
            return View(customer);
        }

        public async Task<IActionResult> Delete(string id)
        {
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
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    await LogActivity(userId, "Delete Customer", "Customer", id);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(string id)
        {
            return _context.Customers.Any(e => e.CustomerID == id);
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
                .Include(c => c.Project)
                .Where(c => c.Project != null && 
                           c.Project.ProjectID == projectID &&
                           (c.CustomerID.StartsWith(projectPrefix) || c.CustomerID.StartsWith(projectPrefix + "-")))
                .ToListAsync();

            // Extract numeric part from existing CustomerIDs (format: "JSC0001" or "JSC-00001" for backward compatibility)
            int maxNumber = 0;
            foreach (var existingCustomer in existingCustomers)
            {
                string existingCustomerID = existingCustomer.CustomerID;
                
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

        private async Task<string> SaveKinFileAsync(string customerId, IFormFile file, string filePrefix)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "customers", customerId, "kin");
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

            return $"/uploads/customers/{customerId}/kin/{fileName}";
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

        // ===========================================================
        // ATTACHMENT MANAGEMENT
        // ===========================================================

        [HttpPost]
        public async Task<IActionResult> UploadAttachment(string customerId, IFormFile file, string attachmentType, string description = "")
        {
            try
            {
                if (string.IsNullOrEmpty(customerId))
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
                var customer = await _context.Customers.FindAsync(customerId);
                if (customer == null)
                {
                    return Json(new { success = false, message = "Customer not found" });
                }

                // Check for existing CustomerPicture or IDCard (only one allowed)
                if (attachmentType == "CustomerPicture" || attachmentType == "IDCard")
                {
                    var existing = await _context.Attachments
                        .FirstOrDefaultAsync(a => a.RefType == "Customer" && 
                                                  a.RefID == customerId && 
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
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "customers", customerId);
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate unique filename
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                var relativePath = $"/uploads/customers/{customerId}/{uniqueFileName}";

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
                    RefID = customerId,
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
