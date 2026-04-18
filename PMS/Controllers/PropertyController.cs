using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS.Data;
using PMS.Models;
using PMS.Services;
using System.Security.Claims;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Hosting;

namespace PMS.Controllers
{
    [Authorize]
    public class PropertyController : Controller
    {
        private const string ModuleKey = "Property";
        private readonly PMSDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IModulePermissionService _modulePermission;

        public PropertyController(PMSDbContext context, IWebHostEnvironment environment, IModulePermissionService modulePermission)
        {
            _context = context;
            _environment = environment;
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
                .FirstOrDefaultAsync(p => p.PropertyID == id);

            if (property == null)
            {
                return NotFound();
            }

            return View(property);
        }

        public async Task<IActionResult> Create()
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
            ViewBag.Projects = _context.Projects.ToList();
            ViewBag.Dealers = _context.Dealers.Where(d => d.Status == "Active").ToList();

            // Plot Type from Configuration
            var plotTypesConfig = _context.Configurations
                .FirstOrDefault(c => c.Category == "PlotTypes" || c.ConfigKey == "PlotTypes");
            ViewBag.PlotTypes = (plotTypesConfig?.ConfigValue != null)
                ? plotTypesConfig.ConfigValue.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList()
                : new List<string> { "Residential", "Commercial", "Industrial", "Agricultural" };

            // Property Type from Configuration
            var propertyTypesConfig = _context.Configurations
                .FirstOrDefault(c => c.ConfigKey == "propertytypes");
            ViewBag.PropertyTypes = (propertyTypesConfig?.ConfigValue != null)
                ? propertyTypesConfig.ConfigValue.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList()
                : new List<string>();

            return View();
        }

        /// <summary>Returns Sizes and PropertyTypes for the selected project (for Property/Create dropdowns).</summary>
        [HttpGet]
        public IActionResult GetProjectDetails(string projectId)
        {
            if (string.IsNullOrWhiteSpace(projectId))
                return Json(new { sizes = Array.Empty<string>(), propertyTypes = Array.Empty<string>() });

            var project = _context.Projects.AsNoTracking().FirstOrDefault(p => p.ProjectID == projectId);
            if (project == null)
                return Json(new { sizes = Array.Empty<string>(), propertyTypes = Array.Empty<string>() });

            var sizes = (project.Sizes ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();
            var propertyTypes = (project.PropertyTypes ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();

            return Json(new { sizes, propertyTypes });
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
            ViewBag.Dealers = _context.Dealers.Where(d => d.Status == "Active").ToList();
            var plotTypesConfig = _context.Configurations.FirstOrDefault(c => c.Category == "PlotTypes" || c.ConfigKey == "PlotTypes");
            ViewBag.PlotTypes = (plotTypesConfig?.ConfigValue != null) ? plotTypesConfig.ConfigValue.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList() : new List<string> { "Residential", "Commercial", "Industrial", "Agricultural" };
            return View(property);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
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
            ViewBag.Dealers = _context.Dealers.Where(d => d.Status == "Active").ToList();
            return View(property);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Property property)
        {
            var denied = await EnsurePermissionAsync("Edit");
            if (denied != null) return denied;
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
            ViewBag.Dealers = _context.Dealers.Where(d => d.Status == "Active").ToList();
            
            // Reload sizes from Configuration table
            var sizesConfig = _context.Configurations
                .FirstOrDefault(c => c.ConfigKey == "sizes");
            ViewBag.Sizes = (sizesConfig?.ConfigValue != null)
                ? sizesConfig.ConfigValue.Split(',').Select(s => s.Trim()).ToList()
                : new List<string>();
            
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
            var denied = await EnsurePermissionAsync("Admin");
            if (denied != null) return denied;
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
            var denied = await EnsurePermissionAsync("Admin");
            if (denied != null) return denied;
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

        public IActionResult Import()
        {
            ViewBag.Dealers = _context.Dealers.Where(d => d.Status == "Active").OrderBy(d => d.DealershipName).ToList();
            ViewBag.Projects = _context.Projects.OrderBy(p => p.ProjectName).ToList();
            return View();
        }

        public IActionResult DownloadSample()
        {
            try
            {
                // Look for the file in wwwroot/samples (standard location for static files)
                // This ensures the file is included when publishing
                var filePath = Path.Combine(_environment.WebRootPath, "samples", "property.xlsx");

                // Fallback to other locations for development
                if (!System.IO.File.Exists(filePath))
                {
                    var possiblePaths = new[]
                    {
                        Path.Combine(_environment.WebRootPath, "samples", "property.xlsx"),
                        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "samples", "property.xlsx"),
                        Path.Combine(_environment.ContentRootPath, "wwwroot", "samples", "property.xlsx"),
                        Path.Combine(Directory.GetCurrentDirectory(), "bin", "property.xlsx"),
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "property.xlsx")
                    };

                    foreach (var path in possiblePaths)
                    {
                        if (System.IO.File.Exists(path))
                        {
                            filePath = path;
                            break;
                        }
                    }
                }

                if (!System.IO.File.Exists(filePath))
                {
                    TempData["Error"] = "Sample file not found. Please contact administrator.";
                    return RedirectToAction(nameof(Import));
                }

                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                var fileName = "property_sample.xlsx";
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error downloading sample file: {ex.Message}";
                return RedirectToAction(nameof(Import));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportPreview(IFormFile file, int dealerId, string projectId)
        {
            ViewBag.SelectedDealerID = dealerId;
            ViewBag.SelectedProjectID = projectId;

            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select a file to upload.";
                ViewBag.Dealers = _context.Dealers.Where(d => d.Status == "Active").OrderBy(d => d.DealershipName).ToList();
                ViewBag.Projects = _context.Projects.OrderBy(p => p.ProjectName).ToList();
                return View("Import");
            }

            if (dealerId <= 0)
            {
                TempData["Error"] = "Please select a dealer.";
                ViewBag.Dealers = _context.Dealers.Where(d => d.Status == "Active").OrderBy(d => d.DealershipName).ToList();
                ViewBag.Projects = _context.Projects.OrderBy(p => p.ProjectName).ToList();
                return View("Import");
            }

            // Verify dealer exists
            var dealer = await _context.Dealers.FindAsync(dealerId);
            if (dealer == null)
            {
                TempData["Error"] = "Selected dealer not found.";
                ViewBag.Dealers = _context.Dealers.Where(d => d.Status == "Active").OrderBy(d => d.DealershipName).ToList();
                ViewBag.Projects = _context.Projects.OrderBy(p => p.ProjectName).ToList();
                return View("Import");
            }

            if (string.IsNullOrWhiteSpace(projectId))
            {
                TempData["Error"] = "Please select a project.";
                ViewBag.Dealers = _context.Dealers.Where(d => d.Status == "Active").OrderBy(d => d.DealershipName).ToList();
                ViewBag.Projects = _context.Projects.OrderBy(p => p.ProjectName).ToList();
                return View("Import");
            }

            var project = await _context.Projects.FirstOrDefaultAsync(p => p.ProjectID == projectId);
            if (project == null)
            {
                TempData["Error"] = "Selected project not found.";
                ViewBag.Dealers = _context.Dealers.Where(d => d.Status == "Active").OrderBy(d => d.DealershipName).ToList();
                ViewBag.Projects = _context.Projects.OrderBy(p => p.ProjectName).ToList();
                return View("Import");
            }

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".csv" && extension != ".xlsx" && extension != ".xls")
            {
                TempData["Error"] = "Please upload a CSV or Excel file (.csv, .xlsx, .xls).";
                return RedirectToAction(nameof(Import));
            }

            List<PropertyImportViewModel> properties = new List<PropertyImportViewModel>();

            try
            {
                if (extension == ".csv")
                {
                    properties = ParseCsvFile(file);
                }
                else
                {
                    properties = ParseExcelFile(file);
                }

                if (properties.Count == 0)
                {
                    TempData["Error"] = "No valid data found in the file. Please check the file format.";
                    return RedirectToAction(nameof(Import));
                }

                foreach (var property in properties)
                {
                    property.ProjectID = projectId;
                    if (string.IsNullOrWhiteSpace(property.PlotNo))
                    {
                        property.IsValid = false;
                        property.ErrorMessage = "PlotNo is required";
                    }
                }

                ViewBag.TotalRows = properties.Count;
                ViewBag.ValidRows = properties.Count(p => p.IsValid);
                ViewBag.InvalidRows = properties.Count(p => !p.IsValid);
                ViewBag.FileName = file.FileName;
                ViewBag.ProjectName = project.ProjectName;
                ViewBag.ProjectID = project.ProjectID;

                // Store in session for confirmation
                HttpContext.Session.SetString("ImportData", System.Text.Json.JsonSerializer.Serialize(properties));
                HttpContext.Session.SetInt32("ImportDealerID", dealerId);
                HttpContext.Session.SetString("ImportProjectID", projectId);
                ViewBag.DealerName = dealer.DealershipName;

                return View(properties);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error processing file: {ex.Message}";
                return RedirectToAction(nameof(Import));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportConfirm()
        {
            var importDataJson = HttpContext.Session.GetString("ImportData");
            var dealerId = HttpContext.Session.GetInt32("ImportDealerID");
            var projectId = HttpContext.Session.GetString("ImportProjectID");
            
            if (string.IsNullOrEmpty(importDataJson))
            {
                TempData["Error"] = "Import session expired. Please upload the file again.";
                return RedirectToAction(nameof(Import));
            }

            if (dealerId == null || dealerId <= 0)
            {
                TempData["Error"] = "Dealer information not found. Please upload the file again.";
                return RedirectToAction(nameof(Import));
            }

            if (string.IsNullOrWhiteSpace(projectId))
            {
                TempData["Error"] = "Project information not found. Please upload the file again.";
                return RedirectToAction(nameof(Import));
            }

            var projectExists = await _context.Projects.AnyAsync(p => p.ProjectID == projectId);
            if (!projectExists)
            {
                TempData["Error"] = "Selected project no longer exists. Please upload the file again.";
                return RedirectToAction(nameof(Import));
            }

            var properties = System.Text.Json.JsonSerializer.Deserialize<List<PropertyImportViewModel>>(importDataJson);
            if (properties == null || properties.Count == 0)
            {
                TempData["Error"] = "No data to import.";
                return RedirectToAction(nameof(Import));
            }

            var validProperties = properties.Where(p => p.IsValid).ToList();
            if (validProperties.Count == 0)
            {
                TempData["Error"] = "No valid properties to import.";
                return RedirectToAction(nameof(Import));
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int importedCount = 0;
            int skippedCount = 0;

            foreach (var prop in validProperties)
            {
                try
                {
                    // Check if property already exists (by ProjectID + PlotNo + Block)
                    var exists = await _context.Properties
                        .AnyAsync(p => p.ProjectID == projectId && 
                                      p.PlotNo == prop.PlotNo && 
                                      p.Block == prop.Block);

                    if (exists)
                    {
                        skippedCount++;
                        continue;
                    }

                    var property = new Property
                    {
                        PropertyID = GenerateID(),
                        ProjectID = projectId!,
                        PlotNo = prop.PlotNo,
                        Street = prop.Street,
                        PlotType = prop.PlotType,
                        Block = prop.Block,
                        Floor = prop.Floor,
                        PropertyType = prop.PropertyType,
                        Size = prop.Size,
                        AdditionalInfo = prop.AdditionalInfo,
                        Status = "Available",
                        DealerID = dealerId.Value,
                        CreatedAt = DateTime.Now
                    };

                    _context.Properties.Add(property);
                    importedCount++;
                }
                catch (Exception)
                {
                    skippedCount++;
                    continue;
                }
            }

            await _context.SaveChangesAsync();

            if (userId != null)
            {
                await LogActivity(userId, $"Import Properties - {importedCount} imported", "Property", "Bulk");
            }

            HttpContext.Session.Remove("ImportData");
            HttpContext.Session.Remove("ImportProjectID");

            TempData["Success"] = $"Successfully imported {importedCount} properties. {skippedCount} skipped (duplicates or errors).";
            return RedirectToAction(nameof(Index));
        }

        private List<PropertyImportViewModel> ParseCsvFile(IFormFile file)
        {
            var properties = new List<PropertyImportViewModel>();
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                string? headerLine = reader.ReadLine();
                if (headerLine == null) return properties;
                var headerColumns = ParseCsvLine(headerLine);
                var hasProjectColumn = headerColumns.Count > 0 &&
                                       string.Equals(headerColumns[0]?.Trim(), "ProjectID", StringComparison.OrdinalIgnoreCase);
                var startIndex = hasProjectColumn ? 1 : 0;

                int rowNumber = 2; // Start from row 2 (after header)
                while (!reader.EndOfStream)
                {
                    string? line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var columns = ParseCsvLine(line);
                    if (columns.Count >= startIndex + 6) // PlotNo to Size required in template; Floor and AdditionalInfo optional
                    {
                        var plotNo = columns[startIndex]?.Trim() ?? string.Empty;
                        var street = columns.Count > startIndex + 1 ? columns[startIndex + 1]?.Trim() ?? string.Empty : string.Empty;
                        var plotType = columns.Count > startIndex + 2 ? columns[startIndex + 2]?.Trim() ?? string.Empty : string.Empty;
                        var block = columns.Count > startIndex + 3 ? columns[startIndex + 3]?.Trim() ?? string.Empty : string.Empty;
                        var propertyType = columns.Count > startIndex + 4 ? columns[startIndex + 4]?.Trim() ?? string.Empty : string.Empty;
                        var size = columns.Count > startIndex + 5 ? columns[startIndex + 5]?.Trim() ?? string.Empty : string.Empty;
                        var floor = columns.Count > startIndex + 6 ? columns[startIndex + 6]?.Trim() ?? string.Empty : string.Empty;
                        var additionalInfo = columns.Count > startIndex + 7 ? columns[startIndex + 7]?.Trim() ?? string.Empty : string.Empty;

                        if (string.IsNullOrWhiteSpace(plotNo) &&
                            string.IsNullOrWhiteSpace(street) &&
                            string.IsNullOrWhiteSpace(plotType) &&
                            string.IsNullOrWhiteSpace(block) &&
                            string.IsNullOrWhiteSpace(propertyType) &&
                            string.IsNullOrWhiteSpace(size) &&
                            string.IsNullOrWhiteSpace(floor) &&
                            string.IsNullOrWhiteSpace(additionalInfo))
                        {
                            rowNumber++;
                            continue;
                        }

                        properties.Add(new PropertyImportViewModel
                        {
                            RowNumber = rowNumber,
                            PlotNo = plotNo,
                            Street = street,
                            PlotType = plotType,
                            Block = block,
                            PropertyType = propertyType,
                            Size = size,
                            Floor = floor,
                            AdditionalInfo = additionalInfo
                        });
                    }
                    rowNumber++;
                }
            }
            return properties;
        }

        private List<string> ParseCsvLine(string line)
        {
            var columns = new List<string>();
            var currentColumn = new System.Text.StringBuilder();
            bool inQuotes = false;

            foreach (char c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == '\t' && !inQuotes) // CSV can be tab-separated
                {
                    columns.Add(currentColumn.ToString());
                    currentColumn.Clear();
                }
                else if (c == ',' && !inQuotes)
                {
                    columns.Add(currentColumn.ToString());
                    currentColumn.Clear();
                }
                else
                {
                    currentColumn.Append(c);
                }
            }
            columns.Add(currentColumn.ToString()); // Add last column

            return columns;
        }

        private List<PropertyImportViewModel> ParseExcelFile(IFormFile file)
        {
            var properties = new List<PropertyImportViewModel>();
            
            try
            {
                using (var stream = file.OpenReadStream())
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheets.First();
                    var headerValue = GetCellValue(worksheet, 1, 1);
                    var hasProjectColumn = string.Equals(headerValue, "ProjectID", StringComparison.OrdinalIgnoreCase);
                    var startColumn = hasProjectColumn ? 2 : 1;
                    var rows = worksheet.RowsUsed().Skip(1); // Skip header row
                    
                    int rowNumber = 2;
                    foreach (var row in rows)
                    {
                        var plotNo = GetCellValue(worksheet, row.RowNumber(), startColumn) ?? string.Empty;
                        var street = GetCellValue(worksheet, row.RowNumber(), startColumn + 1) ?? string.Empty;
                        var plotType = GetCellValue(worksheet, row.RowNumber(), startColumn + 2) ?? string.Empty;
                        var block = GetCellValue(worksheet, row.RowNumber(), startColumn + 3) ?? string.Empty;
                        var propertyType = GetCellValue(worksheet, row.RowNumber(), startColumn + 4) ?? string.Empty;
                        var size = GetCellValue(worksheet, row.RowNumber(), startColumn + 5) ?? string.Empty;
                        var floor = GetCellValue(worksheet, row.RowNumber(), startColumn + 6) ?? string.Empty;
                        var additionalInfo = GetCellValue(worksheet, row.RowNumber(), startColumn + 7) ?? string.Empty;

                        if (string.IsNullOrWhiteSpace(plotNo) &&
                            string.IsNullOrWhiteSpace(street) &&
                            string.IsNullOrWhiteSpace(plotType) &&
                            string.IsNullOrWhiteSpace(block) &&
                            string.IsNullOrWhiteSpace(propertyType) &&
                            string.IsNullOrWhiteSpace(size) &&
                            string.IsNullOrWhiteSpace(floor) &&
                            string.IsNullOrWhiteSpace(additionalInfo))
                        {
                            rowNumber++;
                            continue;
                        }
                        
                        properties.Add(new PropertyImportViewModel
                        {
                            RowNumber = rowNumber,
                            PlotNo = plotNo,
                            Street = street,
                            PlotType = plotType,
                            Block = block,
                            PropertyType = propertyType,
                            Size = size,
                            Floor = floor,
                            AdditionalInfo = additionalInfo
                        });
                        rowNumber++;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading Excel file: {ex.Message}");
            }
            
            return properties;
        }

        private string? GetCellValue(IXLWorksheet worksheet, int row, int column)
        {
            var cell = worksheet.Cell(row, column);
            var value = cell.GetValue<string>();
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        public async Task<IActionResult> Reserve(string projectId = null)
        {
            var query = _context.Properties
                .Include(p => p.Project)
                .Include(p => p.Dealer)
                .Where(p => p.Status == "Available");

            // Filter by project if selected
            if (!string.IsNullOrEmpty(projectId))
            {
                query = query.Where(p => p.ProjectID == projectId);
            }

            var availableProperties = await query
                .OrderBy(p => p.ProjectID)
                .ThenBy(p => p.PlotNo)
                .ToListAsync();

            ViewBag.Dealers = await _context.Dealers
                .Where(d => d.Status == "Active")
                .OrderBy(d => d.DealershipName)
                .ToListAsync();

            ViewBag.Projects = await _context.Projects
                .OrderBy(p => p.ProjectName)
                .ToListAsync();

            ViewBag.SelectedProjectId = projectId;

            return View(availableProperties);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkChangeDealer(List<string> propertyIds, int fromDealerId, int toDealerId, string remarks)
        {
            if (propertyIds == null || propertyIds.Count == 0)
            {
                TempData["Error"] = "Please select at least one property.";
                return RedirectToAction(nameof(Reserve));
            }

            if (toDealerId <= 0)
            {
                TempData["Error"] = "Please select a target dealer.";
                return RedirectToAction(nameof(Reserve));
            }

            // Verify target dealer exists
            var targetDealer = await _context.Dealers.FindAsync(toDealerId);
            if (targetDealer == null)
            {
                TempData["Error"] = "Selected dealer not found.";
                return RedirectToAction(nameof(Reserve));
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "ADMIN";
            int updatedCount = 0;
            int skippedCount = 0;

            foreach (var propertyId in propertyIds)
            {
                try
                {
                    var property = await _context.Properties.FindAsync(propertyId);
                    if (property == null || property.Status != "Available")
                    {
                        skippedCount++;
                        continue;
                    }

                    // Get old dealer name for logging
                    string? oldDealerName = null;
                    if (property.DealerID.HasValue)
                    {
                        var oldDealer = await _context.Dealers.FindAsync(property.DealerID.Value);
                        oldDealerName = oldDealer?.DealershipName;
                    }

                    // Get new dealer name
                    var newDealerName = targetDealer.DealershipName;

                    // Update property
                    property.DealerID = toDealerId;
                    _context.Update(property);

                    // Create log entry
                    var log = new PropertyLog
                    {
                        PropertyID = propertyId,
                        Action = "Dealer Changed",
                        OldValue = oldDealerName ?? $"DealerID: {property.DealerID}",
                        NewValue = newDealerName ?? $"DealerID: {toDealerId}",
                        Remarks = remarks ?? $"Bulk dealer change from {(property.DealerID.HasValue ? oldDealerName : "None")} to {newDealerName}",
                        CreatedAt = DateTime.Now,
                        CreatedBy = userId
                    };

                    _context.PropertyLogs.Add(log);
                    updatedCount++;
                }
                catch (Exception)
                {
                    skippedCount++;
                    continue;
                }
            }

            await _context.SaveChangesAsync();

            // Log activity
            if (userId != null)
            {
                await LogActivity(userId, $"Bulk Dealer Change - {updatedCount} properties updated", "Property", "Bulk");
            }

            TempData["Success"] = $"Successfully updated {updatedCount} properties. {skippedCount} skipped.";
            return RedirectToAction(nameof(Reserve));
        }

        private string GenerateID()
        {
            return Guid.NewGuid().ToString("N")[..10].ToUpper();
        }
    }
}
