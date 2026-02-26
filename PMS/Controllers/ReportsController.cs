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
    public class ReportsController : Controller
    {
        private const string ModuleKey = "Reports";
        private readonly PMSDbContext _context;
        private readonly IModulePermissionService _modulePermission;

        public ReportsController(PMSDbContext context, IModulePermissionService modulePermission)
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

        // 1. Defaulters Report - Customers with overdue payments
        public async Task<IActionResult> Defaulters()
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;
            var today = DateTime.Today;

            // Get all payment schedules that are overdue
            var defaulters = await _context.PaymentSchedules
                .Include(ps => ps.PaymentPlan)
                    .ThenInclude(pp => pp.Customers)
                .Include(ps => ps.Payments)
                .Where(ps => ps.DueDate < today)
                .ToListAsync();

            // Filter to only those without full payment
            var defaultersList = new List<DefaulterReportViewModel>();

            foreach (var schedule in defaulters)
            {
                var totalPaid = schedule.Payments?.Sum(p => p.Amount) ?? 0m;
                var amountDue = schedule.Amount;
                var balance = amountDue - totalPaid;

                if (balance > 0) // Still has outstanding balance
                {
                    var customers = schedule.PaymentPlan?.Customers ?? new List<Customer>();
                    
                    foreach (var customer in customers)
                    {
                        defaultersList.Add(new DefaulterReportViewModel
                        {
                            CustomerID = customer.CustomerID,
                            CustomerName = customer.FullName,
                            Phone = customer.Phone,
                            PlanName = schedule.PaymentPlan?.PlanName,
                            InstallmentNo = schedule.InstallmentNo,
                            DueDate = schedule.DueDate,
                            AmountDue = amountDue,
                            AmountPaid = totalPaid,
                            Balance = balance,
                            DaysOverdue = (today - schedule.DueDate).Days,
                            Status = customer.Status
                        });
                    }
                }
            }

            return View(defaultersList.OrderByDescending(d => d.DaysOverdue).ToList());
        }

        // 2. Not Allotted Report - Customers without property allotment
        public async Task<IActionResult> NotAllotted(string filter = "All")
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;
            ViewBag.Filter = filter;

            // Get all customers
            var customers = await _context.Customers
                .Include(c => c.PaymentPlan)
                    .ThenInclude(pp => pp.PaymentSchedules)
                        .ThenInclude(ps => ps.Payments)
                .ToListAsync();

            // Get customers with allotments
            var allottedCustomerIds = await _context.Allotments
                .Select(a => a.CustomerID)
                .Distinct()
                .ToListAsync();

            // Filter customers who are NOT allotted
            var notAllottedCustomers = customers
                .Where(c => !allottedCustomerIds.Contains(c.CustomerID))
                .ToList();

            var reportList = new List<NotAllottedReportViewModel>();

            foreach (var customer in notAllottedCustomers)
            {
                var paymentPlan = customer.PaymentPlan;
                var totalDue = paymentPlan?.TotalAmount ?? 0m;
                var totalPaid = paymentPlan?.PaymentSchedules?
                    .SelectMany(ps => ps.Payments ?? new List<Payment>())
                    .Where(p => p.CustomerID == customer.CustomerID)
                    .Sum(p => p.Amount) ?? 0m;

                var balance = totalDue - totalPaid;
                var paymentStatus = balance <= 0 ? "Fully Paid" : balance < totalDue ? "Partial" : "Defaulter";

                var model = new NotAllottedReportViewModel
                {
                    CustomerID = customer.CustomerID,
                    CustomerName = customer.FullName,
                    CNIC = customer.CNIC,
                    Phone = customer.Phone,
                    Email = customer.Email,
                    PlanName = paymentPlan?.PlanName,
                    TotalAmount = totalDue,
                    TotalPaid = totalPaid,
                    Balance = balance,
                    PaymentStatus = paymentStatus,
                    CustomerStatus = customer.Status,
                    RegisteredDate = customer.CreatedAt
                };

                // Apply filter
                if (filter == "FullyPaid" && paymentStatus == "Fully Paid")
                {
                    reportList.Add(model);
                }
                else if (filter == "Defaulters" && paymentStatus == "Defaulter")
                {
                    reportList.Add(model);
                }
                else if (filter == "All")
                {
                    reportList.Add(model);
                }
            }

            return View(reportList.OrderBy(r => r.CustomerName).ToList());
        }

        // 3. Blocked Customers Report - Customers with inactive status
        public async Task<IActionResult> BlockedCustomers()
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;
            var blockedCustomers = await _context.Customers
                .Include(c => c.PaymentPlan)
                .Where(c => c.Status != "Active")
                .OrderBy(c => c.FullName)
                .ToListAsync();

            var reportList = blockedCustomers.Select(c => new BlockedCustomerReportViewModel
            {
                CustomerID = c.CustomerID,
                CustomerName = c.FullName,
                CNIC = c.CNIC,
                Phone = c.Phone,
                Email = c.Email,
                Status = c.Status,
                PlanName = c.PaymentPlan?.PlanName,
                RegisteredDate = c.CreatedAt,
                City = c.City,
                Country = c.Country
            }).ToList();

            return View(reportList);
        }

        // 4. Property Report - Allotted vs Not Allotted by Project, Size, Block
        public async Task<IActionResult> PropertyReport(string projectId = null)
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;
            ViewBag.Projects = await _context.Projects.OrderBy(p => p.ProjectName).ToListAsync();
            ViewBag.SelectedProjectId = projectId;

            var query = _context.Properties
                .Include(p => p.Project)
                .Include(p => p.Allotments)
                    .ThenInclude(a => a.Customer)
                .AsQueryable();

            if (!string.IsNullOrEmpty(projectId))
            {
                query = query.Where(p => p.ProjectID == projectId);
            }

            var properties = await query.OrderBy(p => p.Project.ProjectName)
                .ThenBy(p => p.Block)
                .ThenBy(p => p.PlotNo)
                .ToListAsync();

            var reportList = properties.Select(p => new PropertyReportViewModel
            {
                PropertyID = p.PropertyID,
                ProjectName = p.Project?.ProjectName,
                PlotNo = p.PlotNo,
                Block = p.Block,
                Size = p.Size,
                PropertyType = p.PropertyType,
                Status = p.Status,
                IsAllotted = p.Allotments != null && p.Allotments.Any(),
                AllottedTo = p.Allotments?.FirstOrDefault()?.Customer?.FullName,
                AllottedDate = p.Allotments?.FirstOrDefault()?.AllotmentDate,
                Street = p.Street
            }).ToList();

            // Summary Statistics
            ViewBag.TotalProperties = reportList.Count;
            ViewBag.AllottedCount = reportList.Count(p => p.IsAllotted);
            ViewBag.NotAllottedCount = reportList.Count(p => !p.IsAllotted);

            return View(reportList);
        }
    }

    // View Models for Reports
    public class DefaulterReportViewModel
    {
        public string CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string PlanName { get; set; }
        public int? InstallmentNo { get; set; }
        public DateTime DueDate { get; set; }
        public decimal AmountDue { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal Balance { get; set; }
        public int DaysOverdue { get; set; }
        public string Status { get; set; }
    }

    public class NotAllottedReportViewModel
    {
        public string CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string CNIC { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string PlanName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal Balance { get; set; }
        public string PaymentStatus { get; set; }
        public string CustomerStatus { get; set; }
        public DateTime RegisteredDate { get; set; }
    }

    public class BlockedCustomerReportViewModel
    {
        public string CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string CNIC { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public string PlanName { get; set; }
        public DateTime RegisteredDate { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }

    public class PropertyReportViewModel
    {
        public string PropertyID { get; set; }
        public string ProjectName { get; set; }
        public string PlotNo { get; set; }
        public string Block { get; set; }
        public string Size { get; set; }
        public string PropertyType { get; set; }
        public string Status { get; set; }
        public bool IsAllotted { get; set; }
        public string AllottedTo { get; set; }
        public DateTime? AllottedDate { get; set; }
        public string Street { get; set; }
    }
}

