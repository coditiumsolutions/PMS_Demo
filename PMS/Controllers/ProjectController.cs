using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS.Data;
using PMS.Models;
using PMS.Services;
using System.Security.Claims;
using System.Linq;
using System.Collections.Generic;

namespace PMS.Controllers
{
    [Authorize]
    public class ProjectController : Controller
    {
        private const string ModuleKey = "Project";
        private readonly PMSDbContext _context;
        private readonly IModulePermissionService _modulePermission;

        public ProjectController(PMSDbContext context, IModulePermissionService modulePermission)
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

        public async Task<IActionResult> Index()
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;
            var projects = await _context.Projects
                .Include(p => p.Properties)
                .Include(p => p.PaymentPlans)
                .Include(p => p.Ballotings)
                .ToListAsync();
            
            // Get customer counts for each project
            var projectIds = projects.Select(p => p.ProjectID).ToList();
            var customerCounts = await _context.Customers
                .Where(c => c.ProjectID != null && projectIds.Contains(c.ProjectID))
                .GroupBy(c => c.ProjectID!)
                .Select(g => new { ProjectID = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ProjectID, x => x.Count);
            
            ViewBag.CustomerCounts = customerCounts;
            
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

        public async Task<IActionResult> Create()
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
            // Load project types from Configuration table
            var projectTypesConfig = _context.Configurations
                .FirstOrDefault(c => c.ConfigKey == "projecttypes");
            ViewBag.ProjectTypes = projectTypesConfig != null 
                ? projectTypesConfig.ConfigValue.Split(',').Select(s => s.Trim()).ToList()
                : new List<string> { "Residential", "Commercial", "Mixed", "Industrial" };
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Project project)
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
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

            // Reload project types for validation errors
            var projectTypesConfig = _context.Configurations
                .FirstOrDefault(c => c.ConfigKey == "projecttypes");
            ViewBag.ProjectTypes = projectTypesConfig != null 
                ? projectTypesConfig.ConfigValue.Split(',').Select(s => s.Trim()).ToList()
                : new List<string> { "Residential", "Commercial", "Mixed", "Industrial" };

            return View(project);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            // Load project types from Configuration table
            var projectTypesConfig = _context.Configurations
                .FirstOrDefault(c => c.ConfigKey == "projecttypes");
            ViewBag.ProjectTypes = projectTypesConfig != null 
                ? projectTypesConfig.ConfigValue.Split(',').Select(s => s.Trim()).ToList()
                : new List<string> { "Residential", "Commercial", "Mixed", "Industrial" };

            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Project project)
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
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

            // Reload project types for validation errors
            var projectTypesConfig = _context.Configurations
                .FirstOrDefault(c => c.ConfigKey == "projecttypes");
            ViewBag.ProjectTypes = projectTypesConfig != null 
                ? projectTypesConfig.ConfigValue.Split(',').Select(s => s.Trim()).ToList()
                : new List<string> { "Residential", "Commercial", "Mixed", "Industrial" };

            return View(project);
        }

        public async Task<IActionResult> Delete(string id)
        {
            var denied = await EnsurePermissionAsync("Admin");
            if (denied != null) return denied;
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
            var denied = await EnsurePermissionAsync("Admin");
            if (denied != null) return denied;
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
            var denied = await EnsurePermissionAsync("Admin");
            if (denied != null) return denied;
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
            var denied = await EnsurePermissionAsync("Admin");
            if (denied != null) return denied;
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

        /// <summary>Floor Plan: show all projects; optional sub-project filter after selecting a project.</summary>
        [HttpGet]
        public async Task<IActionResult> FloorPlan(string? projectId, string? subProject)
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;

            var allProjects = await _context.Projects
                .OrderBy(p => p.ProjectName)
                .ToListAsync();

            ViewBag.Projects = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                allProjects,
                "ProjectID",
                "ProjectName",
                projectId);

            var selectedProjectMeta = allProjects.FirstOrDefault(p => p.ProjectID == projectId);
            var subProjectOptions = (selectedProjectMeta?.SubProjects ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s)
                .ToList();
            ViewBag.SubProjects = subProjectOptions;

            if (string.IsNullOrEmpty(projectId) || selectedProjectMeta == null)
            {
                return View(new FloorPlanViewModel
                {
                    SelectedProjectID = projectId,
                    SelectedSubProject = subProject
                });
            }

            var project = await _context.Projects
                .Include(p => p.Properties)
                .FirstOrDefaultAsync(p => p.ProjectID == projectId);
            if (project == null)
                return NotFound();

            var selectedSubProject = string.IsNullOrWhiteSpace(subProject) ? null : subProject.Trim();
            var propertiesByFloor = project.Properties
                .Where(p => selectedSubProject == null
                    || string.Equals((p.SubProject ?? string.Empty).Trim(), selectedSubProject, StringComparison.OrdinalIgnoreCase))
                .GroupBy(p =>
                {
                    var floor = (p.Floor ?? string.Empty).Trim();
                    return string.IsNullOrWhiteSpace(floor) ? "No floor mentioned" : floor;
                })
                .Select(g => new FloorGroupViewModel
                {
                    FloorName = g.Key,
                    Properties = g.OrderBy(p => p.PropertyID).ToList()
                })
                .OrderBy(f =>
                {
                    if (f.FloorName == "G") return 0;
                    if (int.TryParse(f.FloorName, out var n)) return n;
                    if (f.FloorName == "No floor mentioned") return 1000;
                    return 999;
                })
                .ToList();

            var model = new FloorPlanViewModel
            {
                Project = project,
                SelectedProjectID = projectId,
                SelectedSubProject = selectedSubProject,
                Floors = propertiesByFloor
            };

            return View(model);
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
