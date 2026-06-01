using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS.Data;

namespace PMS.ViewComponents
{
    public class TopNavBarViewModel
    {
        public string UserDisplayName { get; set; } = string.Empty;
        public DateTime? LoginTime { get; set; }
    }

    public class TopNavBarViewComponent : ViewComponent
    {
        private readonly PMSDbContext _context;

        public TopNavBarViewComponent(PMSDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new TopNavBarViewModel
            {
                UserDisplayName = User.Identity?.Name ?? "User"
            };

            var sessionId = UserClaimsPrincipal.FindFirst("SessionID")?.Value;
            if (!string.IsNullOrEmpty(sessionId))
            {
                var session = await _context.UserSessions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.SessionID == sessionId);

                if (session != null)
                    model.LoginTime = session.LoginTime;
            }

            return View(model);
        }
    }
}
