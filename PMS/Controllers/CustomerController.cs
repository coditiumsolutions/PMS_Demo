using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS.Data;
using PMS.Models;
using System.Security.Claims;

namespace PMS.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly PMSDbContext _context;

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
            // No need for ViewBag.Registrations - using AJAX search instead
            ViewBag.PaymentPlans = _context.PaymentPlans.ToList();
            
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
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                customer.CustomerID = GenerateID();
                customer.CreatedAt = DateTime.Now;
                customer.Status = "Active";

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    await LogActivity(userId, "Create Customer", "Customer", customer.CustomerID);
                }

                return RedirectToAction(nameof(Index));
            }

            // Reload data on validation error
            ViewBag.PaymentPlans = _context.PaymentPlans.ToList();
            
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
            
            return View(customer);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            ViewBag.Registrations = _context.Registrations.ToList();
            ViewBag.PaymentPlans = _context.PaymentPlans.ToList();
            
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
            
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Customer customer)
        {
            if (id != customer.CustomerID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
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
            ViewBag.PaymentPlans = _context.PaymentPlans.ToList();
            
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
    }
}
