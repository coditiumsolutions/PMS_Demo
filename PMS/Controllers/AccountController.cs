using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS.Data;
using PMS.Models;
using PMS.Services;
using System.Security.Claims;
using BCrypt.Net;

namespace PMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly PMSDbContext _context;
        private readonly IModulePermissionService _modulePermission;
        private readonly ISiteConfigService _siteConfigService;

        private static readonly string[] ModuleKeys = new[]
        {
            "Home", "Registration", "Customer", "Transfer", "TransferFee", "NDC", "Project", "Dealer", "Property", "Payment",
            "Allotment", "Rental", "SalesInquiry", "Reports", "Account", "Settings", "ActivityLog",
            "AccountsManagement", "Ticket", "TesSQL", "InquiryApi", "Refund", "Waiver"
        };

        private static readonly string[] PermissionOptions = new[] { "NoAccess", "Read", "Edit", "Admin" };

        public AccountController(PMSDbContext context, IModulePermissionService modulePermission, ISiteConfigService siteConfigService)
        {
            _context = context;
            _modulePermission = modulePermission;
            _siteConfigService = siteConfigService;
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var homePerm = await _modulePermission.GetPermissionAsync(userId, "Home");
                if (_modulePermission.CanRead(homePerm))
                    return RedirectToAction("Index", "Home");
                else
                    return RedirectToAction("Workspace", "Home");
            }
            var siteConfig = await _siteConfigService.GetAsync();
            return View(siteConfig);
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please enter both email and password.";
                var cfg = await _siteConfigService.GetAsync();
                return View(cfg);
            }

            // Case-insensitive email comparison
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == email.ToLower() && u.IsActive);

            if (user != null && !string.IsNullOrEmpty(user.PasswordHash) && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
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
                
                // Configure persistent cookie properties - CRITICAL for staying logged in
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true, // CRITICAL: Makes cookie persistent (survives browser restart)
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30), // 30 days from now
                    AllowRefresh = true, // Allow refreshing expiration on each request (sliding expiration)
                    IssuedUtc = DateTimeOffset.UtcNow,
                    // These ensure the cookie persists across browser sessions
                };

                // Sign in with persistent cookie - creates a cookie that lasts 30 days
                // With SlidingExpiration=true in Program.cs, expiration resets on each request
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity), 
                    authProperties);

                // Create login log in ActivityLog table
                try
                {
                    var loginLog = new ActivityLog
                    {
                        UserID = user.UserID,
                        Action = "Login",
                        RefType = "User",
                        RefID = user.UserID,
                        CreatedAt = DateTime.Now
                    };
                    _context.ActivityLogs.Add(loginLog);
                    await _context.SaveChangesAsync();
                }
                catch
                {
                    // ActivityLog table may not exist, continue anyway
                }

                // Redirect to Dashboard only if user has Home module access; otherwise Workspace
                var homePerm = await _modulePermission.GetPermissionAsync(user.UserID, "Home");
                if (_modulePermission.CanRead(homePerm))
                    return RedirectToAction("Index", "Home");
                else
                    return RedirectToAction("Workspace", "Home");
            }

            ViewBag.Error = "Invalid email or password.";
            var siteConfig = await _siteConfigService.GetAsync();
            return View(siteConfig);
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
                    try
                    {
                        await LogActivity(userId, "Logout", "User", userId);
                    }
                    catch
                    {
                        // ActivityLog table may not exist, continue anyway
                    }
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
                .Include(u => u.UserSessions)
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            // Try to load ActivityLogs separately (table may not exist)
            try
            {
                user.ActivityLogs = await _context.ActivityLogs
                    .Where(a => a.UserID == userId)
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(10)
                    .ToListAsync();
            }
            catch
            {
                // ActivityLog table doesn't exist, set to empty list
                user.ActivityLogs = new List<ActivityLog>();
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
            SetUsersConfigViewBag();
            ViewBag.ModuleKeys = ModuleKeys;
            ViewBag.PermissionOptions = PermissionOptions;
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
                    SetUsersConfigViewBag();
                    ViewBag.ModuleKeys = ModuleKeys;
                    ViewBag.PermissionOptions = PermissionOptions;
                    return View(user);
                }

                user.UserID = GenerateID();
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
                user.CreatedAt = DateTime.Now;
                user.IsActive = true;

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                await SaveModulePermissionsFromFormAsync(user.UserID);

                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    try
                    {
                        await LogActivity(currentUserId, "Create User", "User", user.UserID);
                    }
                    catch
                    {
                        // ActivityLog table may not exist, continue anyway
                    }
                }

                return RedirectToAction("Users");
            }

            ViewBag.Roles = _context.ACLs.ToList();
            SetUsersConfigViewBag();
            ViewBag.ModuleKeys = ModuleKeys;
            ViewBag.PermissionOptions = PermissionOptions;
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
                .Include(u => u.ModulePermissions)
                .FirstOrDefaultAsync(u => u.UserID == id);

            if (user == null)
            {
                return NotFound();
            }

            // Try to load ActivityLogs separately (table may not exist)
            try
            {
                user.ActivityLogs = await _context.ActivityLogs
                    .Where(a => a.UserID == id)
                    .ToListAsync();
            }
            catch
            {
                // ActivityLog table doesn't exist, set to empty list
                user.ActivityLogs = new List<ActivityLog>();
            }

            ViewBag.Roles = _context.ACLs.ToList();
            SetUsersConfigViewBag();
            ViewBag.ModuleKeys = ModuleKeys;
            ViewBag.PermissionOptions = PermissionOptions;
            ViewBag.ModulePermissions = user.ModulePermissions.ToDictionary(p => p.ModuleKey, p => p.Permission);
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
                        SetUsersConfigViewBag();
                        return View(user);
                    }

                    var existing = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserID == user.UserID);
                    if (existing == null) return NotFound();
                    user.PasswordHash = existing.PasswordHash;
                    user.CreatedAt = existing.CreatedAt;
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();

                    await SaveModulePermissionsFromFormAsync(user.UserID);

                    var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(currentUserId))
                    {
                        try
                        {
                            await LogActivity(currentUserId, "Update User", "User", user.UserID);
                        }
                        catch
                        {
                            // ActivityLog table may not exist, continue anyway
                        }
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
            SetUsersConfigViewBag();
            ViewBag.ModuleKeys = ModuleKeys;
            ViewBag.PermissionOptions = PermissionOptions;
            ViewBag.ModulePermissions = await _context.UserModulePermissions.Where(p => p.UserID == user.UserID).ToDictionaryAsync(p => p.ModuleKey, p => p.Permission);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResetUserPassword(string userId, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(userId))
                return NotFound();
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                TempData["ResetPasswordError"] = "Password must be at least 6 characters.";
                return RedirectToAction(nameof(EditUser), new { id = userId });
            }
            if (newPassword != confirmPassword)
            {
                TempData["ResetPasswordError"] = "Passwords do not match.";
                return RedirectToAction(nameof(EditUser), new { id = userId });
            }
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Password has been reset successfully.";
            return RedirectToAction(nameof(EditUser), new { id = userId });
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
                    try
                    {
                        await LogActivity(currentUserId, 
                            user.IsActive ? "Activate User" : "Deactivate User", 
                            "User", user.UserID);
                    }
                    catch
                    {
                        // ActivityLog table may not exist, continue anyway
                    }
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

        private async Task SaveModulePermissionsFromFormAsync(string userId)
        {
            var existing = await _context.UserModulePermissions.Where(p => p.UserID == userId).ToListAsync();
            _context.UserModulePermissions.RemoveRange(existing);
            foreach (var key in ModuleKeys)
            {
                var val = Request.Form["ModulePermissions_" + key].FirstOrDefault();
                if (string.IsNullOrEmpty(val)) val = "NoAccess";
                if (val != "NoAccess" && val != "Read" && val != "Edit" && val != "Admin") val = "NoAccess";
                _context.UserModulePermissions.Add(new UserModulePermission
                {
                    UserID = userId,
                    ModuleKey = key,
                    Permission = val
                });
            }
            await _context.SaveChangesAsync();
        }

        private void SetUsersConfigViewBag()
        {
            var deptConfig = _context.Configurations.FirstOrDefault(c => c.ConfigKey == "departments" && (c.Category == "Users" || c.Category == null));
            ViewBag.Departments = deptConfig?.ConfigValue != null
                ? deptConfig.ConfigValue.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList()
                : new List<string>();
            var desigConfig = _context.Configurations.FirstOrDefault(c => c.ConfigKey == "designations" && (c.Category == "Users" || c.Category == null));
            ViewBag.Designations = desigConfig?.ConfigValue != null
                ? desigConfig.ConfigValue.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList()
                : new List<string>();
        }

        private async Task LogActivity(string userId, string action, string refType, string refId)
        {
            try
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
            catch (Exception)
            {
                // ActivityLog table doesn't exist or other error occurred
                // Silently ignore - this is called from try-catch blocks in calling methods
                // Do not rethrow the exception
            }
        }

        private string GenerateID()
        {
            return Guid.NewGuid().ToString("N")[..10].ToUpper();
        }

        // Temporary endpoint to create admin user - Remove after use
        [HttpGet]
        public async Task<IActionResult> CreateAdminUser()
        {
            try
            {
                // Ensure Admin role exists
                var adminRole = await _context.ACLs.FirstOrDefaultAsync(r => r.RoleID == "ADMIN001");
                if (adminRole == null)
                {
                    adminRole = new ACL
                    {
                        RoleID = "ADMIN001",
                        RoleName = "Admin",
                        Permissions = "All"
                    };
                    _context.ACLs.Add(adminRole);
                    await _context.SaveChangesAsync();
                }

                // Check if user already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "abbas@pms.com");
                if (existingUser != null)
                {
                    // Update existing user
                    existingUser.FullName = "Abbas";
                    existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
                    existingUser.RoleID = "ADMIN001";
                    existingUser.IsActive = true;
                    await _context.SaveChangesAsync();
                    return Content($"User updated successfully! Email: abbas@pms.com, Password: Admin@123, UserID: {existingUser.UserID}");
                }

                // Generate unique UserID
                string userId = "USER00002";
                int attempts = 0;
                while (await _context.Users.AnyAsync(u => u.UserID == userId) && attempts < 100)
                {
                    attempts++;
                    var random = new Random();
                    userId = "USER" + random.Next(10000, 99999).ToString();
                }

                // Create the user
                var user = new User
                {
                    UserID = userId,
                    FullName = "Abbas",
                    Email = "abbas@pms.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    RoleID = "ADMIN001",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Content($"User created successfully! Email: abbas@pms.com, Password: Admin@123, UserID: {userId}");
            }
            catch (Exception ex)
            {
                return Content($"Error creating user: {ex.Message}<br/>Stack Trace: {ex.StackTrace}");
            }
        }

        // Temporary debug endpoint to list all users - Remove after use
        [HttpGet]
        public async Task<IActionResult> ListUsers()
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.Role)
                    .ToListAsync();

                var html = "<h2>All Users in Database:</h2><table border='1' cellpadding='5'><tr><th>UserID</th><th>Email</th><th>FullName</th><th>Role</th><th>IsActive</th></tr>";
                
                foreach (var user in users)
                {
                    html += $"<tr><td>{user.UserID}</td><td>{user.Email}</td><td>{user.FullName}</td><td>{user.Role?.RoleName ?? "No Role"}</td><td>{user.IsActive}</td></tr>";
                }
                
                html += "</table>";
                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                return Content($"Error listing users: {ex.Message}");
            }
        }
    }
}
