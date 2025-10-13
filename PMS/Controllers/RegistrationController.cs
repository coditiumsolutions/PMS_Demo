using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS.Data;
using PMS.Models;
using System.Security.Claims;

namespace PMS.Controllers
{
    [Authorize]
    public class RegistrationController : Controller
    {
        private readonly PMSDbContext _context;

        public RegistrationController(PMSDbContext context)
        {
            _context = context;
        }

        // GET: Registration/Index
        public async Task<IActionResult> Index(string statusFilter = "All", string searchTerm = "")
        {
            var registrationsQuery = _context.Registrations
                .Include(r => r.Customers)
                .AsQueryable();

            // Apply status filter
            if (statusFilter != "All" && !string.IsNullOrEmpty(statusFilter))
            {
                registrationsQuery = registrationsQuery.Where(r => r.Status == statusFilter);
            }

            // Apply search
            if (!string.IsNullOrEmpty(searchTerm))
            {
                registrationsQuery = registrationsQuery.Where(r =>
                    r.RegID.Contains(searchTerm) ||
                    r.FullName.Contains(searchTerm) ||
                    r.CNIC.Contains(searchTerm) ||
                    r.Phone.Contains(searchTerm) ||
                    r.Email.Contains(searchTerm)
                );
            }

            var registrations = await registrationsQuery
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            // Statistics
            ViewBag.TotalRegistrations = await _context.Registrations.CountAsync();
            ViewBag.PendingRegistrations = await _context.Registrations.Where(r => r.Status == "Pending").CountAsync();
            ViewBag.ApprovedRegistrations = await _context.Registrations.Where(r => r.Status == "Approved").CountAsync();
            ViewBag.RejectedRegistrations = await _context.Registrations.Where(r => r.Status == "Rejected").CountAsync();
            ViewBag.ConvertedToCustomers = await _context.Customers.CountAsync();

            ViewBag.StatusFilter = statusFilter;
            ViewBag.SearchTerm = searchTerm;

            return View(registrations);
        }

        // GET: Registration/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var registration = await _context.Registrations
                .Include(r => r.Customers)
                .FirstOrDefaultAsync(r => r.RegID == id);

            if (registration == null)
            {
                return NotFound();
            }

            return View(registration);
        }

        // GET: Registration/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Registration/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Registration registration)
        {
            if (ModelState.IsValid)
            {
                registration.RegID = GenerateID();
                registration.CreatedAt = DateTime.Now;
                registration.Status = "Pending";

                _context.Registrations.Add(registration);
                await _context.SaveChangesAsync();

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    await LogActivity(userId, "Create Registration", "Registration", registration.RegID);
                }

                TempData["Success"] = $"Registration created successfully! Registration ID: {registration.RegID}";
                return RedirectToAction(nameof(Index));
            }

            return View(registration);
        }

        // GET: Registration/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var registration = await _context.Registrations.FindAsync(id);
            if (registration == null)
            {
                return NotFound();
            }

            return View(registration);
        }

        // POST: Registration/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Registration registration)
        {
            if (id != registration.RegID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(registration);
                    await _context.SaveChangesAsync();

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(userId))
                    {
                        await LogActivity(userId, "Update Registration", "Registration", registration.RegID);
                    }

                    TempData["Success"] = "Registration updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RegistrationExists(registration.RegID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(registration);
        }

        // POST: Registration/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(string regID, string status)
        {
            var registration = await _context.Registrations.FindAsync(regID);
            if (registration == null)
            {
                TempData["Error"] = "Registration not found";
                return RedirectToAction(nameof(Index));
            }

            registration.Status = status;
            await _context.SaveChangesAsync();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await LogActivity(userId, $"Update Registration Status to {status}", "Registration", regID);
            }

            TempData["Success"] = $"Registration status updated to {status}";
            return RedirectToAction(nameof(Details), new { id = regID });
        }

        // GET: Registration/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var registration = await _context.Registrations
                .Include(r => r.Customers)
                .FirstOrDefaultAsync(r => r.RegID == id);

            if (registration == null)
            {
                return NotFound();
            }

            // Check if registration has customers
            if (registration.Customers != null && registration.Customers.Any())
            {
                TempData["Error"] = "Cannot delete registration. It is linked to customer(s).";
                return RedirectToAction(nameof(Index));
            }

            return View(registration);
        }

        // POST: Registration/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var registration = await _context.Registrations
                .Include(r => r.Customers)
                .FirstOrDefaultAsync(r => r.RegID == id);

            if (registration == null)
            {
                return NotFound();
            }

            // Double check if registration has customers
            if (registration.Customers != null && registration.Customers.Any())
            {
                TempData["Error"] = "Cannot delete registration. It is linked to customer(s).";
                return RedirectToAction(nameof(Index));
            }

            _context.Registrations.Remove(registration);
            await _context.SaveChangesAsync();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await LogActivity(userId, "Delete Registration", "Registration", id);
            }

            TempData["Success"] = "Registration deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private bool RegistrationExists(string id)
        {
            return _context.Registrations.Any(e => e.RegID == id);
        }

        private string GenerateID()
        {
            var lastRegistration = _context.Registrations
                .OrderByDescending(r => r.RegID)
                .FirstOrDefault();

            int nextId = 1;
            if (lastRegistration != null && lastRegistration.RegID.Length > 3)
            {
                int.TryParse(lastRegistration.RegID.Substring(3), out nextId);
                nextId++;
            }

            return "REG" + nextId.ToString("D7");
        }

        private async Task LogActivity(string userId, string action, string refType, string refId)
        {
            var log = new ActivityLog
            {
                UserID = userId,
                Action = action,
                RefType = refType,
                RefID = refId,
                CreatedAt = DateTime.Now
            };
            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}

