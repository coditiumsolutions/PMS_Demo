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
    public class TicketController : Controller
    {
        private const string ModuleKey = "Ticket";
        private readonly PMSDbContext _context;
        private readonly IModulePermissionService _modulePermission;

        public TicketController(PMSDbContext context, IModulePermissionService modulePermission)
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

        private static string GenerateTicketId()
        {
            return Guid.NewGuid().ToString("N")[..10].ToUpper();
        }

        // GET: Ticket list (customer care / CRO)
        public async Task<IActionResult> Index(string status = "All")
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;
            ViewBag.SelectedStatus = status;

            var query = _context.Tickets.AsQueryable();
            if (status != "All")
                query = query.Where(t => t.Status == status);

            var tickets = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();

            ViewBag.TotalTickets = await _context.Tickets.CountAsync();
            ViewBag.PendingTickets = await _context.Tickets.CountAsync(t => t.Status == "Pending");
            ViewBag.AssignedTickets = await _context.Tickets.CountAsync(t => t.Status == "Assigned");
            ViewBag.OngoingTickets = await _context.Tickets.CountAsync(t => t.Status == "Ongoing");
            ViewBag.DiscardedTickets = await _context.Tickets.CountAsync(t => t.Status == "Discarded");
            ViewBag.DuplicateTickets = await _context.Tickets.CountAsync(t => t.Status == "Duplicate");
            ViewBag.ResolvedTickets = await _context.Tickets.CountAsync(t => t.Status == "Resolved");

            ViewBag.Users = await _context.Users
                .Where(u => u.IsActive && u.FullName != null && u.FullName != "")
                .OrderBy(u => u.FullName)
                .Select(u => u.FullName!)
                .ToListAsync();

            return View(tickets);
        }

        // POST: Create ticket (CRO records customer query/complaint)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string? customerID, string? email, string? contact, string? title, string? description)
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
            var ticketId = GenerateTicketId();
            while (await _context.Tickets.AnyAsync(t => t.TicketID == ticketId))
                ticketId = GenerateTicketId();

            var createdBy = User.Identity?.Name ?? User.FindFirst(ClaimTypes.Name)?.Value ?? "CRO";

            var ticket = new Ticket
            {
                TicketID = ticketId,
                CustomerID = customerID?.Trim().NullIfEmpty(),
                Email = email?.Trim().NullIfEmpty(),
                Contact = contact?.Trim().NullIfEmpty(),
                Title = title?.Trim().NullIfEmpty(),
                Description = description?.Trim().NullIfEmpty(),
                Status = "Pending",
                CreatedBy = createdBy,
                CreatedAt = DateTime.Now
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Ticket #{ticket.TicketID} created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Update status
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(string ticketId, string status)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.TicketID == ticketId);
            if (ticket == null) return NotFound();

            ticket.Status = status;
            if (status == "Resolved" || status == "Discarded" || status == "Duplicate")
                ticket.TicketClosingDate = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Status updated.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Add CRO comments
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCROComments(string ticketId, string croComments)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.TicketID == ticketId);
            if (ticket == null) return NotFound();

            var timestamp = DateTime.Now.ToString("MMM dd, yyyy hh:mm tt");
            var userName = User.Identity?.Name ?? "CRO";
            var newComment = $"[{timestamp} - {userName}]\n{croComments?.Trim()}\n\n";
            ticket.CROComments = (ticket.CROComments ?? "") + newComment;
            await _context.SaveChangesAsync();

            TempData["Success"] = "CRO comments added.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Assign to
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTo(string ticketId, string assignedTo)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.TicketID == ticketId);
            if (ticket == null) return NotFound();

            ticket.AssignedTo = string.IsNullOrWhiteSpace(assignedTo) ? null : assignedTo.Trim();
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Ticket assigned to {ticket.AssignedTo ?? "Unassigned"}.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string ticketId)
        {
            var denied = await EnsurePermissionAsync("Admin");
            if (denied != null) return denied;
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.TicketID == ticketId);
            if (ticket == null) return NotFound();

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Ticket deleted.";
            return RedirectToAction(nameof(Index));
        }
    }

    internal static class StringExt
    {
        public static string? NullIfEmpty(this string? s) => string.IsNullOrWhiteSpace(s) ? null : s;
    }
}
