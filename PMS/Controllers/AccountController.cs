using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS.Data;
using PMS.Models;
using System.Security.Claims;
using BCrypt.Net;

namespace PMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly PMSDbContext _context;

        public AccountController(PMSDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please enter both email and password.";
                return View();
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                // Create user session
                var session = new UserSession
                {
                    SessionID = GenerateID(),
                    UserID = user.UserID,
                    IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    DeviceInfo = Request.Headers["User-Agent"].ToString()
                };

                _context.UserSessions.Add(session);
                await _context.SaveChangesAsync();

                // Create claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserID),
                    new Claim(ClaimTypes.Name, user.FullName ?? ""),
                    new Claim(ClaimTypes.Email, user.Email ?? ""),
                    new Claim("SessionID", session.SessionID)
                };

                if (user.Role != null)
                {
                    claims.Add(new Claim(ClaimTypes.Role, user.Role.RoleName ?? ""));
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = false,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity), authProperties);

                // Log activity
                await LogActivity(user.UserID, "Login", "User", user.UserID);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid email or password.";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var sessionId = User.FindFirst("SessionID")?.Value;

                if (!string.IsNullOrEmpty(sessionId))
                {
                    var session = await _context.UserSessions.FindAsync(sessionId);
                    if (session != null)
                    {
                        session.LogoutTime = DateTime.Now;
                        await _context.SaveChangesAsync();
                    }
                }

                if (!string.IsNullOrEmpty(userId))
                {
                    await LogActivity(userId, "Logout", "User", userId);
                }
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.ActivityLogs)
                .Include(u => u.UserSessions)
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            return View(user);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .ToListAsync();
            return View(users);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateUser()
        {
            ViewBag.Roles = _context.ACLs.ToList();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser(User user)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
                if (existingUser != null)
                {
                    ViewBag.Error = "Email already exists.";
                    ViewBag.Roles = _context.ACLs.ToList();
                    return View(user);
                }

                user.UserID = GenerateID();
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
                user.CreatedAt = DateTime.Now;
                user.IsActive = true;

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    await LogActivity(currentUserId, "Create User", "User", user.UserID);
                }

                return RedirectToAction("Users");
            }

            ViewBag.Roles = _context.ACLs.ToList();
            return View(user);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.UserSessions)
                .Include(u => u.ActivityLogs)
                .FirstOrDefaultAsync(u => u.UserID == id);

            if (user == null)
            {
                return NotFound();
            }

            ViewBag.Roles = _context.ACLs.ToList();
            return View(user);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditUser(User user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if email already exists for another user
                    var existingUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email == user.Email && u.UserID != user.UserID);
                    if (existingUser != null)
                    {
                        ViewBag.Error = "Email already exists for another user.";
                        ViewBag.Roles = _context.ACLs.ToList();
                        return View(user);
                    }

                    _context.Update(user);
                    await _context.SaveChangesAsync();

                    var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(currentUserId))
                    {
                        await LogActivity(currentUserId, "Update User", "User", user.UserID);
                    }

                    TempData["Success"] = "User updated successfully.";
                    return RedirectToAction("Users");
                }
                catch (Exception)
                {
                    ViewBag.Error = "An error occurred while updating the user.";
                }
            }

            ViewBag.Roles = _context.ACLs.ToList();
            return View(user);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleUserStatus(string userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                user.IsActive = !user.IsActive;
                await _context.SaveChangesAsync();

                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    await LogActivity(currentUserId, 
                        user.IsActive ? "Activate User" : "Deactivate User", 
                        "User", user.UserID);
                }

                return Json(new { 
                    success = true, 
                    message = $"User {(user.IsActive ? "activated" : "deactivated")} successfully.",
                    isActive = user.IsActive
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
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
