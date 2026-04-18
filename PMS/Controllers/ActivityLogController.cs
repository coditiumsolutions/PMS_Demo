using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PMS.Data;
using PMS.Models;
using PMS.Services;
using System.Data;
using System.Security.Claims;

namespace PMS.Controllers
{
    [Authorize]
    public class ActivityLogController : Controller
    {
        private const string ModuleKey = "ActivityLog";
        private readonly PMSDbContext _context;
        private readonly IModulePermissionService _modulePermission;

        public ActivityLogController(PMSDbContext context, IModulePermissionService modulePermission)
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

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? dateFrom, DateTime? dateTo, string? userId, int page = 1, int pageSize = 50)
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;
            if (page < 1) page = 1;
            if (pageSize < 10) pageSize = 10;
            if (pageSize > 200) pageSize = 200;

            var whereClauses = new List<string>();
            var parameters = new List<object>();
            var paramIndex = 0;

            if (dateFrom.HasValue)
            {
                whereClauses.Add($"CreatedAt >= @p{paramIndex}");
                parameters.Add(dateFrom.Value.Date);
                paramIndex++;
            }
            if (dateTo.HasValue)
            {
                whereClauses.Add($"CreatedAt < @p{paramIndex}");
                parameters.Add(dateTo.Value.Date.AddDays(1));
                paramIndex++;
            }
            if (!string.IsNullOrWhiteSpace(userId))
            {
                whereClauses.Add($"UserID = @p{paramIndex}");
                parameters.Add(userId.Trim());
                paramIndex++;
            }

            var whereSql = whereClauses.Count > 0 ? " WHERE " + string.Join(" AND ", whereClauses) : "";

            var conn = _context.Database.GetDbConnection();
            await conn.OpenAsync();

            try
            {
                var totalCount = 0;
                using (var cmdCount = conn.CreateCommand())
                {
                    cmdCount.CommandText = "SELECT COUNT(*) FROM ActivityLog" + whereSql;
                    for (var i = 0; i < parameters.Count; i++)
                    {
                        var p = cmdCount.CreateParameter();
                        p.ParameterName = $"@p{i}";
                        p.Value = parameters[i];
                        cmdCount.Parameters.Add(p);
                    }
                    var obj = await cmdCount.ExecuteScalarAsync();
                    totalCount = obj != null && obj != DBNull.Value ? Convert.ToInt32(obj) : 0;
                }

                var totalPages = totalCount == 0 ? 1 : (int)Math.Ceiling(totalCount / (double)pageSize);
                if (page > totalPages) page = totalPages;
                var offset = (page - 1) * pageSize;

                var logs = new List<ActivityLogRow>();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $@"
SELECT a.LogID, a.CreatedAt, a.UserID, a.Action, a.RefType, a.RefID, u.FullName, a.Details
FROM ActivityLog a
LEFT JOIN Users u ON a.UserID = u.UserID
{whereSql}
ORDER BY a.CreatedAt DESC, a.LogID DESC
OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY";
                    for (var i = 0; i < parameters.Count; i++)
                    {
                        var p = cmd.CreateParameter();
                        p.ParameterName = $"@p{i}";
                        p.Value = parameters[i];
                        cmd.Parameters.Add(p);
                    }

                    using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        logs.Add(new ActivityLogRow
                        {
                            LogId = reader.GetInt32(0),
                            CreatedAt = reader.IsDBNull(1) ? default : reader.GetDateTime(1),
                            UserId = reader.IsDBNull(2) ? null : reader.GetString(2),
                            UserName = reader.IsDBNull(6) || string.IsNullOrEmpty(reader.GetString(6)) ? (reader.IsDBNull(2) ? "—" : reader.GetString(2)) : reader.GetString(6),
                            Action = reader.IsDBNull(3) ? null : reader.GetString(3),
                            RefType = reader.IsDBNull(4) ? null : reader.GetString(4),
                            RefId = reader.IsDBNull(5) ? null : reader.GetString(5),
                            Details = !reader.IsDBNull(7) ? reader.GetString(7) : null
                        });
                    }
                }

                var usersForFilter = await _context.ActivityLogs
                    .Where(a => a.UserID != null)
                    .Select(a => a.UserID!)
                    .Distinct()
                    .Join(_context.Users, uid => uid, u => u.UserID, (uid, u) => new UserOption { UserId = uid, Label = u.FullName ?? uid })
                    .OrderBy(x => x.Label)
                    .ToListAsync();

                var vm = new ActivityLogIndexViewModel
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    Logs = logs,
                    Users = usersForFilter,
                    Actions = new List<string>(),
                    RefTypes = new List<string>(),
                    UserId = userId,
                    FromDate = dateFrom,
                    ToDate = dateTo
                };

                return View(vm);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    await conn.CloseAsync();
            }
        }

        /// <summary>Blocking logs list with search filters (CustomerID, UserID, date range, NewStatus).</summary>
        [HttpGet]
        public async Task<IActionResult> BlockingLogs(string? customerId, string? userId, DateTime? dateFrom, DateTime? dateTo, string? newStatus, int page = 1, int pageSize = 25)
        {
            var denied = await EnsurePermissionAsync("Read");
            if (denied != null) return denied;
            if (page < 1) page = 1;
            if (pageSize < 10) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = _context.BlockingLogs
                .Include(b => b.Customer)
                .Include(b => b.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(customerId))
            {
                var cid = customerId.Trim();
                query = query.Where(b => b.CustomerID != null && b.CustomerID.Contains(cid));
            }
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var uid = userId.Trim();
                query = query.Where(b => b.UserID != null && b.UserID.Contains(uid));
            }
            if (dateFrom.HasValue)
                query = query.Where(b => b.ActionDate >= dateFrom.Value.Date);
            if (dateTo.HasValue)
                query = query.Where(b => b.ActionDate < dateTo.Value.Date.AddDays(1));
            if (!string.IsNullOrWhiteSpace(newStatus))
            {
                var status = newStatus.Trim();
                query = query.Where(b => b.NewStatus != null && b.NewStatus == status);
            }

            var totalCount = await query.CountAsync();
            var totalPages = totalCount == 0 ? 1 : (int)Math.Ceiling(totalCount / (double)pageSize);
            if (page > totalPages) page = totalPages;

            var list = await query
                .OrderByDescending(b => b.ActionDate)
                .ThenByDescending(b => b.BlockingLogID)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CustomerId = customerId ?? "";
            ViewBag.UserId = userId ?? "";
            ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd") ?? "";
            ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd") ?? "";
            ViewBag.NewStatus = newStatus ?? "";
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = totalPages;
            ViewBag.List = list;
            return View();
        }
    }
}
