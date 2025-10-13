using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS.Data;
using PMS.Models;
using System.Security.Claims;

namespace PMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SettingsController : Controller
    {
        private readonly PMSDbContext _context;

        public SettingsController(PMSDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var configurations = await _context.Configurations
                .Include(c => c.UpdatedByUser)
                .OrderBy(c => c.Category)
                .ThenBy(c => c.ConfigKey)
                .ToListAsync();
            
            return View(configurations);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Configuration configuration)
        {
            if (ModelState.IsValid)
            {
                // Check if key already exists
                var existing = await _context.Configurations.FindAsync(configuration.ConfigKey);
                if (existing != null)
                {
                    ViewBag.Error = "Configuration key already exists.";
                    return View(configuration);
                }

                configuration.CreatedAt = DateTime.Now;
                configuration.UpdatedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                configuration.UpdatedAt = DateTime.Now;

                _context.Configurations.Add(configuration);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Configuration added successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(configuration);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var configuration = await _context.Configurations.FindAsync(id);
            if (configuration == null)
            {
                return NotFound();
            }

            return View(configuration);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Configuration configuration)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    configuration.UpdatedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    configuration.UpdatedAt = DateTime.Now;

                    _context.Update(configuration);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Configuration updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    ViewBag.Error = "An error occurred while updating the configuration.";
                }
            }

            return View(configuration);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var configuration = await _context.Configurations.FindAsync(id);
            if (configuration != null)
            {
                _context.Configurations.Remove(configuration);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Configuration deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetByCategory(string category)
        {
            var configs = await _context.Configurations
                .Where(c => c.Category == category)
                .Select(c => c.ConfigValue)
                .ToListAsync();

            return Json(configs);
        }
    }
}

