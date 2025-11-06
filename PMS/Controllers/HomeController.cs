using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS.Data;
using PMS.Models;

namespace PMS.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PMSDbContext _context;

        public HomeController(ILogger<HomeController> logger, PMSDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Now.Date;
            
            // Get property status distribution
            var propertyStatusData = await _context.Properties
                .GroupBy(p => p.Status ?? "Unknown")
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            // Get last 6 months payment data
            var sixMonthsAgo = DateTime.Now.AddMonths(-6);
            var monthlyPayments = await _context.Payments
                .Where(p => p.PaymentDate >= sixMonthsAgo && p.Status == "Completed")
                .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                .Select(g => new { 
                    Year = g.Key.Year, 
                    Month = g.Key.Month, 
                    Total = g.Sum(p => (decimal?)p.Amount) ?? 0 
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            // Get payment status counts
            var schedules = await _context.PaymentSchedules
                .Include(s => s.Payments)
                .ToListAsync();
            
            var overdue = schedules.Count(s => s.DueDate < today && 
                (s.Payments == null || (s.Payments.Where(p => p.Status == "Completed").Sum(p => (decimal?)p.Amount) ?? 0) < s.Amount));
            var dueThisWeek = schedules.Count(s => s.DueDate >= today && s.DueDate <= today.AddDays(7) &&
                (s.Payments == null || (s.Payments.Where(p => p.Status == "Completed").Sum(p => (decimal?)p.Amount) ?? 0) < s.Amount));
            var upcoming = schedules.Count(s => s.DueDate > today.AddDays(7) &&
                (s.Payments == null || (s.Payments.Where(p => p.Status == "Completed").Sum(p => (decimal?)p.Amount) ?? 0) < s.Amount));
            var paid = schedules.Count(s => s.Payments != null && 
                (s.Payments.Where(p => p.Status == "Completed").Sum(p => (decimal?)p.Amount) ?? 0) >= s.Amount);

            // Get customer registration trend
            var customerTrend = await _context.Customers
                .Where(c => c.CreatedAt >= sixMonthsAgo)
                .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month })
                .Select(g => new { 
                    Year = g.Key.Year, 
                    Month = g.Key.Month, 
                    Count = (int?)g.Count() ?? 0 
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            // Get dealer data for graph
            var dealers = await _context.Dealers
                .Include(d => d.Customers)
                .Include(d => d.Properties)
                .ToListAsync();

            // Sort in memory after loading (can't use null operators in expression trees)
            var sortedDealers = dealers
                .OrderByDescending(d => (d.Customers != null ? d.Customers.Count : 0) + (d.Properties != null ? d.Properties.Count : 0))
                .Take(10)
                .ToList();

            var dealerData = sortedDealers.Select(d => new DealerDashboardData
            {
                DealershipName = d.DealershipName ?? string.Empty,
                Customers = d.Customers != null ? d.Customers.Count : 0,
                Properties = d.Properties != null ? d.Properties.Count : 0
            }).ToList();

            var dashboardData = new DashboardViewModel
            {
                TotalCustomers = await _context.Customers.CountAsync(),
                TotalProjects = await _context.Projects.CountAsync(),
                TotalProperties = await _context.Properties.CountAsync(),
                AvailableProperties = await _context.Properties.CountAsync(p => p.Status == "Available"),
                AllottedProperties = await _context.Properties.CountAsync(p => p.Status == "Allotted"),
                TotalPayments = await _context.Payments.AnyAsync() ? await _context.Payments.SumAsync(p => p.Amount) : 0,
                RecentCustomers = await _context.Customers
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(5)
                    .ToListAsync(),
                RecentPayments = await _context.Payments
                    .Include(p => p.PaymentSchedule)
                        .ThenInclude(ps => ps.PaymentPlan)
                    .OrderByDescending(p => p.PaymentDate)
                    .Take(5)
                    .ToListAsync(),
                PendingAllotments = await _context.Allotments
                    .Include(a => a.Customer)
                    .Include(a => a.Property)
                    .Where(a => a.WorkFlowStatus == "Pending")
                    .Take(5)
                    .ToListAsync(),
                
                // Chart Data
                PropertyStatusData = propertyStatusData.ToDictionary(x => x.Status ?? "Unknown", x => x.Count),
                MonthlyPaymentsData = monthlyPayments.ToDictionary(
                    x => new DateTime(x.Year, x.Month, 1).ToString("MMM yyyy"), 
                    x => x.Total),
                PaymentStatusData = new Dictionary<string, int>
                {
                    { "Overdue", overdue },
                    { "Due This Week", dueThisWeek },
                    { "Upcoming", upcoming },
                    { "Paid", paid }
                },
                CustomerTrendData = customerTrend.ToDictionary(
                    x => new DateTime(x.Year, x.Month, 1).ToString("MMM yyyy"), 
                    x => x.Count),
                DealerData = dealerData
            };

            return View(dashboardData);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
