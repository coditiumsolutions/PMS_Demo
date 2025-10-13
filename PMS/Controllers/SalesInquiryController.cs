using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS.Data;
using PMS.Models;
using System.Security.Claims;

namespace PMS.Controllers
{
    [Authorize]
    public class SalesInquiryController : Controller
    {
        private readonly PMSDbContext _context;

        public SalesInquiryController(PMSDbContext context)
        {
            _context = context;
        }

        // GET: Sales Inquiries List
        public async Task<IActionResult> Index(string status = "All")
        {
            ViewBag.SelectedStatus = status;

            var query = _context.PropertyInquiries.AsQueryable();

            if (status != "All")
            {
                query = query.Where(i => i.Status == status);
            }

            var inquiries = await query
                .OrderByDescending(i => i.SubmittedAt)
                .ToListAsync();

            // Statistics
            ViewBag.TotalInquiries = await _context.PropertyInquiries.CountAsync();
            ViewBag.NewInquiries = await _context.PropertyInquiries.CountAsync(i => i.Status == "New");
            ViewBag.ContactedInquiries = await _context.PropertyInquiries.CountAsync(i => i.IsContacted);
            ViewBag.ConvertedInquiries = await _context.PropertyInquiries.CountAsync(i => i.Status == "Converted");

            return View(inquiries);
        }

        // POST: Update Status
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int inquiryId, string status)
        {
            var inquiry = await _context.PropertyInquiries.FindAsync(inquiryId);
            
            if (inquiry == null)
            {
                return NotFound();
            }

            inquiry.Status = status;
            
            if (status == "Contacted" || status == "Follow-up")
            {
                inquiry.IsContacted = true;
            }

            await _context.SaveChangesAsync();

            // Log activity
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await LogActivity(userId, $"Updated Inquiry Status to {status}", "PropertyInquiry", inquiryId.ToString());
            }

            TempData["Success"] = "Status updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Mark as Contacted
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsContacted(int inquiryId, string method)
        {
            var inquiry = await _context.PropertyInquiries.FindAsync(inquiryId);
            
            if (inquiry == null)
            {
                return NotFound();
            }

            inquiry.IsContacted = true;
            inquiry.Status = "Contacted";
            
            if (string.IsNullOrEmpty(inquiry.Notes))
            {
                inquiry.Notes = $"Contacted via {method} on {DateTime.Now:MMM dd, yyyy hh:mm tt}";
            }
            else
            {
                inquiry.Notes += $"\n\nContacted via {method} on {DateTime.Now:MMM dd, yyyy hh:mm tt}";
            }

            await _context.SaveChangesAsync();

            // Log activity
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await LogActivity(userId, $"Contacted customer via {method}", "PropertyInquiry", inquiryId.ToString());
            }

            TempData["Success"] = $"Marked as contacted via {method}!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Add Notes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNotes(int inquiryId, string notes)
        {
            var inquiry = await _context.PropertyInquiries.FindAsync(inquiryId);
            
            if (inquiry == null)
            {
                return NotFound();
            }

            var timestamp = DateTime.Now.ToString("MMM dd, yyyy hh:mm tt");
            var userName = User.Identity?.Name ?? "Unknown";
            
            if (string.IsNullOrEmpty(inquiry.Notes))
            {
                inquiry.Notes = $"[{timestamp} - {userName}]\n{notes}";
            }
            else
            {
                inquiry.Notes += $"\n\n[{timestamp} - {userName}]\n{notes}";
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Notes added successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Assign To
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTo(int inquiryId, string assignedTo)
        {
            var inquiry = await _context.PropertyInquiries.FindAsync(inquiryId);
            
            if (inquiry == null)
            {
                return NotFound();
            }

            inquiry.AssignedTo = assignedTo;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Inquiry assigned to {assignedTo}!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Set Follow-up Date
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetFollowUp(int inquiryId, DateTime followUpDate)
        {
            var inquiry = await _context.PropertyInquiries.FindAsync(inquiryId);
            
            if (inquiry == null)
            {
                return NotFound();
            }

            inquiry.FollowUpDate = followUpDate;
            inquiry.Status = "Follow-up";
            await _context.SaveChangesAsync();

            TempData["Success"] = "Follow-up date set successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Delete Inquiry
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int inquiryId)
        {
            var inquiry = await _context.PropertyInquiries.FindAsync(inquiryId);
            
            if (inquiry == null)
            {
                return NotFound();
            }

            _context.PropertyInquiries.Remove(inquiry);
            await _context.SaveChangesAsync();

            // Log activity
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await LogActivity(userId, "Deleted Property Inquiry", "PropertyInquiry", inquiryId.ToString());
            }

            TempData["Success"] = "Inquiry deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // Helper method to log activities
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
    }
}

