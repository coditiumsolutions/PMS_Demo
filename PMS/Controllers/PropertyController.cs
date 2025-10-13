using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS.Data;
using PMS.Models;
using System.Security.Claims;

namespace PMS.Controllers
{
    [Authorize]
    public class PropertyController : Controller
    {
        private readonly PMSDbContext _context;

        public PropertyController(PMSDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var properties = await _context.Properties
                .Include(p => p.Project)
                .Include(p => p.Allotments)
                    .ThenInclude(a => a.Customer)
                .ToListAsync();
            return View(properties);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var property = await _context.Properties
                .Include(p => p.Project)
                .Include(p => p.Allotments)
                    .ThenInclude(a => a.Customer)
                .Include(p => p.Possessions)
                    .ThenInclude(po => po.Customer)
                .Include(p => p.Transfers)
                .FirstOrDefaultAsync(p => p.PropertyID == id);

            if (property == null)
            {
                return NotFound();
            }

            return View(property);
        }

        public IActionResult Create()
        {
            ViewBag.Projects = _context.Projects.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Property property)
        {
            if (ModelState.IsValid)
            {
                property.PropertyID = GenerateID();
                property.CreatedAt = DateTime.Now;
                property.Status = "Available";

                _context.Properties.Add(property);
                await _context.SaveChangesAsync();

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    await LogActivity(userId, "Create Property", "Property", property.PropertyID);
                }

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Projects = _context.Projects.ToList();
            return View(property);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var property = await _context.Properties.FindAsync(id);
            if (property == null)
            {
                return NotFound();
            }

            ViewBag.Projects = _context.Projects.ToList();
            return View(property);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Property property)
        {
            if (id != property.PropertyID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(property);
                    await _context.SaveChangesAsync();

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(userId))
                    {
                        await LogActivity(userId, "Update Property", "Property", property.PropertyID);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PropertyExists(property.PropertyID))
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

            ViewBag.Projects = _context.Projects.ToList();
            return View(property);
        }

        [HttpGet]
        public async Task<IActionResult> Allot(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var property = await _context.Properties
                .Include(p => p.Project)
                .FirstOrDefaultAsync(p => p.PropertyID == id);

            if (property == null)
            {
                return NotFound();
            }

            ViewBag.Customers = _context.Customers.ToList();
            ViewBag.Property = property;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Allot(string propertyId, string customerId, string allotmentType, string comments)
        {
            if (string.IsNullOrEmpty(propertyId) || string.IsNullOrEmpty(customerId))
            {
                return BadRequest();
            }

            var allotment = new Allotment
            {
                AllotmentID = GenerateID(),
                PropertyID = propertyId,
                CustomerID = customerId,
                AllottedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                AllotmentDate = DateTime.Now,
                AllottmentType = allotmentType,
                WorkFlowStatus = "Pending",
                Comments = comments
            };

            _context.Allotments.Add(allotment);

            // Update property status
            var property = await _context.Properties.FindAsync(propertyId);
            if (property != null)
            {
                property.Status = "Allotted";
                _context.Update(property);
            }

            await _context.SaveChangesAsync();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await LogActivity(userId, "Allot Property", "Allotment", allotment.AllotmentID);
            }

            return RedirectToAction(nameof(Details), new { id = propertyId });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var property = await _context.Properties
                .Include(p => p.Project)
                .Include(p => p.Allotments)
                    .ThenInclude(a => a.Customer)
                .FirstOrDefaultAsync(p => p.PropertyID == id);

            if (property == null)
            {
                return NotFound();
            }

            return View(property);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var property = await _context.Properties.FindAsync(id);
            if (property != null)
            {
                _context.Properties.Remove(property);
                await _context.SaveChangesAsync();

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    await LogActivity(userId, "Delete Property", "Property", id);
                }

                TempData["Success"] = "Property deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PropertyExists(string id)
        {
            return _context.Properties.Any(e => e.PropertyID == id);
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
