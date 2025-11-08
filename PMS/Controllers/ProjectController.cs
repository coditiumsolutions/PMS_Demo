using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS.Data;
using PMS.Models;
using System.Security.Claims;

namespace PMS.Controllers
{
    [Authorize]
    public class ProjectController : Controller
    {
        private readonly PMSDbContext _context;

        public ProjectController(PMSDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var projects = await _context.Projects
                .Include(p => p.Properties)
                .Include(p => p.PaymentPlans)
                .Include(p => p.Ballotings)
                .ToListAsync();
            return View(projects);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Properties)
                .Include(p => p.PaymentPlans)
                .Include(p => p.Ballotings)
                .FirstOrDefaultAsync(p => p.ProjectID == id);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Project project)
        {
            // Check if Prefix already exists
            if (!string.IsNullOrEmpty(project.Prefix))
            {
                var existingProject = await _context.Projects
                    .FirstOrDefaultAsync(p => p.Prefix.ToUpper() == project.Prefix.ToUpper());
                
                if (existingProject != null)
                {
                    ModelState.AddModelError("Prefix", "A project with this Prefix already exists. Please use a different Prefix.");
                }
            }

            if (ModelState.IsValid)
            {
                project.ProjectID = GenerateID();
                project.CreatedAt = DateTime.Now;

                _context.Projects.Add(project);
                await _context.SaveChangesAsync();

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    await LogActivity(userId, "Create Project", "Project", project.ProjectID);
                }

                return RedirectToAction(nameof(Index));
            }

            return View(project);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Project project)
        {
            if (id != project.ProjectID)
            {
                return NotFound();
            }

            // Check if Prefix already exists (excluding current project)
            if (!string.IsNullOrEmpty(project.Prefix))
            {
                var existingProject = await _context.Projects
                    .FirstOrDefaultAsync(p => p.Prefix.ToUpper() == project.Prefix.ToUpper() && p.ProjectID != project.ProjectID);
                
                if (existingProject != null)
                {
                    ModelState.AddModelError("Prefix", "A project with this Prefix already exists. Please use a different Prefix.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(project);
                    await _context.SaveChangesAsync();

                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(userId))
                    {
                        await LogActivity(userId, "Update Project", "Project", project.ProjectID);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.ProjectID))
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

            return View(project);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    await LogActivity(userId, "Delete Project", "Project", id);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ConductBalloting(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            ViewBag.Project = project;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConductBalloting(string projectId, string remarks)
        {
            if (string.IsNullOrEmpty(projectId))
            {
                return BadRequest();
            }

            var balloting = new Balloting
            {
                BallotID = GenerateID(),
                ProjectID = projectId,
                ConductedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                ConductedAt = DateTime.Now,
                Remarks = remarks
            };

            _context.Ballotings.Add(balloting);
            await _context.SaveChangesAsync();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await LogActivity(userId, "Conduct Balloting", "Balloting", balloting.BallotID);
            }

            return RedirectToAction(nameof(Details), new { id = projectId });
        }

        private bool ProjectExists(string id)
        {
            return _context.Projects.Any(e => e.ProjectID == id);
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
