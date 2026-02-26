using System.Diagnostics;
using System.Text.Json;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using PMS.Data;
using PMS.Models;

namespace PMS.Controllers
{
    public class PropertyStatusResult
    {
        public string? Status { get; set; }
        public int Count { get; set; }
    }

    public class AIChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }

    public class AIChatResponse
    {
        public List<AIChatStep> Steps { get; set; } = new List<AIChatStep>();
    }

    public class AIChatStep
    {
        public int StepNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = "pending"; // pending, completed, error
        public string Content { get; set; } = string.Empty;
        public bool IsSQL { get; set; }
        public string? SQLQuery { get; set; }
        public List<Dictionary<string, object>>? Data { get; set; }
    }

    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly PMSDbContext _context;

        public HomeController(ILogger<HomeController> logger, PMSDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // #region agent log
            try { System.IO.File.AppendAllText(@"d:\PMS\PMS\PMS\.cursor\debug.log", JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "HomeController.cs:28", message = "Index method entry", data = new { }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion
            try
            {
                var today = DateTime.Now.Date;
                var sixMonthsAgo = DateTime.Now.AddMonths(-6);
                
                // Get property status distribution (handle missing tables)
                Dictionary<string, int> propertyStatusDict = new Dictionary<string, int>();
                try
                {
                    // Use raw SQL to avoid EF relationship validation
                    var propertyStatusData = await _context.Database
                        .SqlQueryRaw<PropertyStatusResult>("SELECT ISNULL(Status, 'Unknown') as Status, COUNT(*) as Count FROM Property GROUP BY Status")
                        .ToListAsync();
                    
                    propertyStatusDict = propertyStatusData.ToDictionary(x => x.Status ?? "Unknown", x => x.Count);
                }
                catch
                {
                    // Properties table doesn't exist or has relationship issues
                    propertyStatusDict = new Dictionary<string, int> { { "Unknown", 0 } };
                }

            // Get last 6 months payment data (handle missing Payments table)
            Dictionary<string, decimal> monthlyPaymentsData = new Dictionary<string, decimal>();
            try
            {
                // Check if Payments table exists first using try-catch
                int tableExists = 0;
                try
                {
                    tableExists = await _context.Database
                        .SqlQueryRaw<int>("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Payments'")
                        .FirstOrDefaultAsync();
                }
                catch
                {
                    // INFORMATION_SCHEMA query failed, assume table doesn't exist
                    tableExists = 0;
                }
                
                if (tableExists > 0)
                {
                    try
                    {
                        // #region agent log
                        try { System.IO.File.AppendAllText(@"d:\PMS\PMS\PMS\.cursor\debug.log", JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "HomeController.cs:74", message = "Before accessing _context.Payments", data = new { tableExists }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                        // #endregion
                        var monthlyPayments = await _context.Payments
                            .AsNoTracking()
                            .Where(p => p.PaymentDate >= sixMonthsAgo && p.Status == "Completed")
                            .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                            .Select(g => new { 
                                Year = g.Key.Year, 
                                Month = g.Key.Month, 
                                Total = g.Sum(p => (decimal?)p.Amount) ?? 0 
                            })
                            .OrderBy(x => x.Year).ThenBy(x => x.Month)
                            .ToListAsync();
                        
                        monthlyPaymentsData = monthlyPayments.ToDictionary(
                            x => new DateTime(x.Year, x.Month, 1).ToString("MMM yyyy"), 
                            x => x.Total);
                    }
                    catch (Exception ex1)
                    {
                        // #region agent log
                        try { System.IO.File.AppendAllText(@"d:\PMS\PMS\PMS\.cursor\debug.log", JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "HomeController.cs:90", message = "Exception in monthlyPayments query", data = new { error = ex1.Message, type = ex1.GetType().Name }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                        // #endregion
                        // Payments query failed
                    }
                }
            }
            catch (Exception ex2)
            {
                // #region agent log
                try { System.IO.File.AppendAllText(@"d:\PMS\PMS\PMS\.cursor\debug.log", JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "HomeController.cs:96", message = "Exception in monthlyPayments outer catch", data = new { error = ex2.Message, type = ex2.GetType().Name }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion
                // Payments table doesn't exist or query failed
            }

            // Get payment status counts (handle missing PaymentSchedules/Payments tables)
            var overdue = 0;
            var dueThisWeek = 0;
            var upcoming = 0;
            var paid = 0;
            try
            {
                // Don't include Payments to avoid querying the Payments table
                var schedules = await _context.PaymentSchedules
                    .AsNoTracking()
                    .ToListAsync();
                
                // Try to load payments separately if table exists
                Dictionary<string, decimal> paymentTotals = new Dictionary<string, decimal>();
                try
                {
                    // Check if Payments table exists first using try-catch
                    int tableExists = 0;
                    try
                    {
                        tableExists = await _context.Database
                            .SqlQueryRaw<int>("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Payments'")
                            .FirstOrDefaultAsync();
                    }
                    catch
                    {
                        // INFORMATION_SCHEMA query failed, assume table doesn't exist
                        tableExists = 0;
                    }
                    
                if (tableExists > 0)
                {
                    try
                    {
                        // #region agent log
                        try { System.IO.File.AppendAllText(@"d:\PMS\PMS\PMS\.cursor\debug.log", JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "HomeController.cs:135", message = "Before accessing _context.Payments for paymentTotals", data = new { tableExists }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                        // #endregion
                        var payments = await _context.Payments
                            .AsNoTracking()
                            .Where(p => p.Status == "Completed")
                                .GroupBy(p => p.ScheduleID)
                                .Select(g => new { ScheduleID = g.Key, Total = g.Sum(p => (decimal?)p.Amount) ?? 0 })
                                .ToListAsync();
                            
                            paymentTotals = payments.Where(p => p.ScheduleID != null)
                                .ToDictionary(p => p.ScheduleID!, p => p.Total);
                        }
                        catch
                        {
                            // Payments query failed
                        }
                    }
                }
                catch
                {
                    // Payments table doesn't exist
                }
                
                overdue = schedules.Count(s => s.DueDate < today && 
                    (!paymentTotals.ContainsKey(s.ScheduleID) || paymentTotals[s.ScheduleID] < s.Amount));
                dueThisWeek = schedules.Count(s => s.DueDate >= today && s.DueDate <= today.AddDays(7) &&
                    (!paymentTotals.ContainsKey(s.ScheduleID) || paymentTotals[s.ScheduleID] < s.Amount));
                upcoming = schedules.Count(s => s.DueDate > today.AddDays(7) &&
                    (!paymentTotals.ContainsKey(s.ScheduleID) || paymentTotals[s.ScheduleID] < s.Amount));
                paid = schedules.Count(s => paymentTotals.ContainsKey(s.ScheduleID) && 
                    paymentTotals[s.ScheduleID] >= s.Amount);
            }
            catch
            {
                // PaymentSchedules table doesn't exist
            }

            // Get customer registration trend (handle missing tables)
            Dictionary<string, int> customerTrendDict = new Dictionary<string, int>();
            try
            {
                var customerTrend = await _context.Customers
                    .AsNoTracking()
                    .Where(c => c.CreatedAt >= sixMonthsAgo)
                    .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month })
                    .Select(g => new { 
                        Year = g.Key.Year, 
                        Month = g.Key.Month, 
                        Count = (int?)g.Count() ?? 0 
                    })
                    .OrderBy(x => x.Year).ThenBy(x => x.Month)
                    .ToListAsync();
                
                customerTrendDict = customerTrend.ToDictionary(
                    x => new DateTime(x.Year, x.Month, 1).ToString("MMM yyyy"), 
                    x => x.Count);
            }
            catch
            {
                // Customers table doesn't exist
            }

            // Get dealer data for graph (handle missing tables)
            List<DealerDashboardData> dealerData = new List<DealerDashboardData>();
            try
            {
                // Load dealers without includes to avoid querying related tables that might not exist
                var dealers = await _context.Dealers
                    .AsNoTracking()
                    .ToListAsync();
                
                // Try to load related data separately
                Dictionary<int, int> dealerCustomerCounts = new Dictionary<int, int>();
                Dictionary<int, int> dealerPropertyCounts = new Dictionary<int, int>();
                
                try
                {
                    dealerCustomerCounts = await _context.Customers
                        .AsNoTracking()
                        .Where(c => c.DealerID.HasValue)
                        .GroupBy(c => c.DealerID!.Value)
                        .ToDictionaryAsync(g => g.Key, g => g.Count());
                }
                catch { }
                
                try
                {
                    dealerPropertyCounts = await _context.Properties
                        .AsNoTracking()
                        .Where(p => p.DealerID.HasValue)
                        .GroupBy(p => p.DealerID!.Value)
                        .ToDictionaryAsync(g => g.Key, g => g.Count());
                }
                catch { }

                // Sort in memory after loading
                var sortedDealers = dealers
                    .OrderByDescending(d => 
                    {
                        var dealerId = d.DealerID;
                        var customerCount = dealerCustomerCounts.ContainsKey(dealerId) ? dealerCustomerCounts[dealerId] : 0;
                        var propertyCount = dealerPropertyCounts.ContainsKey(dealerId) ? dealerPropertyCounts[dealerId] : 0;
                        return customerCount + propertyCount;
                    })
                    .Take(10)
                    .ToList();

                dealerData = sortedDealers.Select(d => 
                {
                    var dealerId = d.DealerID;
                    return new DealerDashboardData
                    {
                        DealershipName = d.DealershipName ?? string.Empty,
                        Customers = dealerCustomerCounts.ContainsKey(dealerId) ? dealerCustomerCounts[dealerId] : 0,
                        Properties = dealerPropertyCounts.ContainsKey(dealerId) ? dealerPropertyCounts[dealerId] : 0
                    };
                }).ToList();
            }
            catch
            {
                // Dealers table doesn't exist
            }

            // Get TotalPayments and RecentPayments (handle missing Payments table)
            decimal totalPayments = 0;
            List<Payment> recentPayments = new List<Payment>();
            try
            {
                // Check if Payments table exists first using try-catch
                int tableExists = 0;
                try
                {
                    tableExists = await _context.Database
                        .SqlQueryRaw<int>("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Payments'")
                        .FirstOrDefaultAsync();
                }
                catch
                {
                    // INFORMATION_SCHEMA query failed, assume table doesn't exist
                    tableExists = 0;
                }
                
                if (tableExists > 0)
                {
                    try
                    {
                        // #region agent log
                        try { System.IO.File.AppendAllText(@"d:\PMS\PMS\PMS\.cursor\debug.log", JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "HomeController.cs:279", message = "Before accessing _context.Payments for TotalPayments", data = new { tableExists }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                        // #endregion
                        if (await _context.Payments.AsNoTracking().AnyAsync())
                        {
                            totalPayments = await _context.Payments.AsNoTracking().SumAsync(p => p.Amount);
                        }
                        // Don't include related entities to avoid querying missing tables
                        recentPayments = await _context.Payments
                            .AsNoTracking()
                            .OrderByDescending(p => p.PaymentDate)
                            .Take(5)
                            .ToListAsync();
                    }
                    catch (Exception ex3)
                    {
                        // #region agent log
                        try { System.IO.File.AppendAllText(@"d:\PMS\PMS\PMS\.cursor\debug.log", JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "HomeController.cs:290", message = "Exception in TotalPayments query", data = new { error = ex3.Message, type = ex3.GetType().Name }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                        // #endregion
                        // Payments query failed
                    }
                }
            }
            catch
            {
                // Payments table doesn't exist
            }

            // Get dashboard counts (handle missing tables)
            int totalCustomers = 0;
            int totalProjects = 0;
            int totalProperties = 0;
            int availableProperties = 0;
            int allottedProperties = 0;
            List<Customer> recentCustomers = new List<Customer>();
            
            try
            {
                totalCustomers = await _context.Customers.AsNoTracking().CountAsync();
                totalProjects = await _context.Projects.AsNoTracking().CountAsync();
                totalProperties = await _context.Properties.AsNoTracking().CountAsync();
                availableProperties = await _context.Properties.AsNoTracking().CountAsync(p => p.Status == "Available");
                allottedProperties = await _context.Properties.AsNoTracking().CountAsync(p => p.Status == "Allotted");
                recentCustomers = await _context.Customers
                    .AsNoTracking()
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(5)
                    .ToListAsync();
            }
            catch
            {
                // Tables don't exist
            }

                var dashboardData = new DashboardViewModel
                {
                    TotalCustomers = totalCustomers,
                    TotalProjects = totalProjects,
                    TotalProperties = totalProperties,
                    AvailableProperties = availableProperties,
                    AllottedProperties = allottedProperties,
                    TotalPayments = totalPayments,
                    RecentCustomers = recentCustomers,
                    RecentPayments = recentPayments,
                    
                    // Chart Data
                    PropertyStatusData = propertyStatusDict,
                    MonthlyPaymentsData = monthlyPaymentsData,
                    PaymentStatusData = new Dictionary<string, int>
                    {
                        { "Overdue", overdue },
                        { "Due This Week", dueThisWeek },
                        { "Upcoming", upcoming },
                        { "Paid", paid }
                    },
                    CustomerTrendData = customerTrendDict,
                    DealerData = dealerData
                };

                // #region agent log
                try { System.IO.File.AppendAllText(@"d:\PMS\PMS\PMS\.cursor\debug.log", JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "HomeController.cs:352", message = "Index method success - returning dashboard", data = new { }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion
                return View(dashboardData);
            }
            catch (Exception ex)
            {
                // #region agent log
                try { System.IO.File.AppendAllText(@"d:\PMS\PMS\PMS\.cursor\debug.log", JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "HomeController.cs:354", message = "Exception in Index outer catch", data = new { error = ex.Message, type = ex.GetType().Name, stackTrace = ex.StackTrace }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion
                // If any database error occurs, return a minimal dashboard
                var dashboardData = new DashboardViewModel
                {
                    TotalCustomers = 0,
                    TotalProjects = 0,
                    TotalProperties = 0,
                    AvailableProperties = 0,
                    AllottedProperties = 0,
                    TotalPayments = 0,
                    RecentCustomers = new List<Customer>(),
                    RecentPayments = new List<Payment>(),
                    PropertyStatusData = new Dictionary<string, int> { { "Unknown", 0 } },
                    MonthlyPaymentsData = new Dictionary<string, decimal>(),
                    PaymentStatusData = new Dictionary<string, int>
                    {
                        { "Overdue", 0 },
                        { "Due This Week", 0 },
                        { "Upcoming", 0 },
                        { "Paid", 0 }
                    },
                    CustomerTrendData = new Dictionary<string, int>(),
                    DealerData = new List<DealerDashboardData>()
                };
                // #region agent log
                try { System.IO.File.AppendAllText(@"d:\PMS\PMS\PMS\.cursor\debug.log", JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "HomeController.cs:380", message = "Index method returning default dashboard after exception", data = new { }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion
                return View(dashboardData);
            }
        }

        public IActionResult Workspace()
        {
            return View();
        }

        public IActionResult AIAssistant()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AIChat([FromBody] AIChatRequest request)
        {
            try
            {
                var schemaPath = Path.Combine(Directory.GetCurrentDirectory(), "DBforAI.txt");
                var groqService = new Services.GroqAIService();
                var schemaService = new Services.DatabaseSchemaService(schemaPath);
                var validatorService = new Services.SQLQueryValidatorService(schemaService);

                var response = new AIChatResponse
                {
                    Steps = new List<AIChatStep>()
                };

                // Step 1: Understand question and form SQL query
                var step1Prompt = $@"You are a SQL query generator. Analyze the user's question and determine if it requires database data.

Database Schema:
{schemaService.GetSchemaSummary()}

User Question: {request.Message}

If the question requires database data, generate a valid SQL Server query. Return ONLY the SQL query, nothing else.
If the question does not require database data, return 'NO_QUERY' followed by a helpful response.

Important: Only return the SQL query or 'NO_QUERY' with response.";

                var step1Result = await groqService.ChatAsync(request.Message, step1Prompt);
                var step1Trimmed = step1Result.Trim();
                var isSQLQuery = step1Trimmed.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) ||
                                 step1Trimmed.StartsWith("INSERT", StringComparison.OrdinalIgnoreCase) ||
                                 step1Trimmed.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase) ||
                                 step1Trimmed.StartsWith("DELETE", StringComparison.OrdinalIgnoreCase);
                var isNoQuery = step1Trimmed.StartsWith("NO_QUERY", StringComparison.OrdinalIgnoreCase);
                
                response.Steps.Add(new AIChatStep
                {
                    StepNumber = 1,
                    Title = "Understanding Question",
                    Status = "completed",
                    Content = step1Result,
                    IsSQL = isSQLQuery && !isNoQuery
                });

                await Task.Delay(2000); // 2 second delay

                // Step 2: Validate and correct SQL query using AI (no rule-based validation)
                // Only proceed if it's actually a SQL query (not NO_QUERY and starts with SQL keyword)
                if (isSQLQuery && !isNoQuery)
                {
                    var sqlQuery = step1Result.Trim();
                    // Clean up the query (remove markdown code blocks if present)
                    sqlQuery = System.Text.RegularExpressions.Regex.Replace(sqlQuery, @"```sql|```", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();
                    
                    // Use AI to validate and correct the query intelligently
                    var validationPrompt = $@"You are a SQL Server query validator. Validate this query against the database schema.

ORIGINAL QUERY:
{sqlQuery}

DATABASE SCHEMA:
{schemaService.GetSchemaSummary()}

YOUR TASK:
1. Check if the query is syntactically correct and uses valid table/column names
2. If the query references a column that doesn't exist in the FROM table, check if a JOIN is needed
3. Fix ONLY actual errors - do NOT change valid table names

ABSOLUTE RULES - NEVER VIOLATE THESE:
1. TABLE NAMES ARE CASE-SENSITIVE IN SQL SERVER
2. The schema contains these EXACT table names: Projects, Customers, Property, PaymentPlan, Payments, Allotment, Dealers, PaymentSchedule
3. If original query says 'Customers', NEVER change it to 'Customer' - 'Customers' is CORRECT
4. If original query says 'Projects', NEVER change it to 'Project' - 'Projects' is CORRECT  
5. If original query says 'Payments', NEVER change it to 'Payment' - 'Payments' is CORRECT
6. PRESERVE table names exactly as written in original query if they match schema (case-sensitive)
7. Only fix: missing JOINs, wrong column references, syntax errors
8. DO NOT change valid table names even if you think singular sounds better

EXAMPLES:
- Original: 'FROM Customers' → Keep as 'FROM Customers' (DO NOT change to 'Customer')
- Original: 'FROM Projects' → Keep as 'FROM Projects' (DO NOT change to 'Project')
- Error: Column 'ProjectName' not in 'Customers' table → Add JOIN to Projects table, keep 'Customers' as-is

RESPONSE FORMAT:
- If query is VALID: Return 'VALID: ' followed by the exact original query
- If query needs FIXING: Return 'CORRECTED: ' followed by the corrected query
- Return ONLY the prefix and SQL query, nothing else";

                    var validationResult = await groqService.ChatAsync(validationPrompt, "You are a SQL Server expert. CRITICAL: Preserve valid table names exactly as written. 'Customers' stays 'Customers', 'Projects' stays 'Projects'. Only fix actual errors like missing JOINs or wrong columns.");
                    validationResult = System.Text.RegularExpressions.Regex.Replace(validationResult, @"```sql|```", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();
                    
                    string finalQuery = sqlQuery;
                    string stepTitle = "Validating Query";
                    string stepContent = "Query is valid ✓";
                    bool wasCorrected = false;
                    
                    if (validationResult.StartsWith("VALID:", StringComparison.OrdinalIgnoreCase))
                    {
                        // Query is valid, extract it
                        finalQuery = validationResult.Substring(6).Trim();
                        if (string.IsNullOrWhiteSpace(finalQuery))
                            finalQuery = sqlQuery;
                    }
                    else if (validationResult.StartsWith("CORRECTED:", StringComparison.OrdinalIgnoreCase))
                    {
                        // Query was corrected
                        finalQuery = validationResult.Substring(10).Trim();
                        wasCorrected = true;
                        stepTitle = "Validating & Correcting Query";
                        stepContent = $"Query validated and corrected:\n\n{finalQuery}";
                    }
                    else
                    {
                        // No prefix, assume it's the corrected query or same query
                        finalQuery = validationResult;
                        if (finalQuery.Equals(sqlQuery, StringComparison.OrdinalIgnoreCase))
                        {
                            // Same as original, consider it valid
                            stepContent = "Query is valid ✓";
                        }
                        else
                        {
                            wasCorrected = true;
                            stepTitle = "Validating & Correcting Query";
                            stepContent = $"Query validated and corrected:\n\n{finalQuery}";
                        }
                    }
                    
                    // Final safety: if corrected query is different but original was likely correct, prefer original
                    if (wasCorrected && !finalQuery.Equals(sqlQuery, StringComparison.OrdinalIgnoreCase))
                    {
                        // Check if original query's table names exist in schema
                        var originalTables = System.Text.RegularExpressions.Regex.Matches(sqlQuery, @"(?:FROM|JOIN)\s+(\w+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
                            .Cast<System.Text.RegularExpressions.Match>()
                            .Select(m => m.Groups[1].Value)
                            .Where(t => !new[] { "SELECT", "WHERE", "GROUP", "ORDER", "HAVING", "ON", "AS", "INNER", "LEFT", "RIGHT", "OUTER" }.Contains(t.ToUpper()))
                            .ToList();
                        
                        bool originalTablesValid = originalTables.All(t => schemaService.TableExists(t));
                        if (originalTablesValid)
                        {
                            // Original tables are valid, check if correction changed them incorrectly
                            var correctedTables = System.Text.RegularExpressions.Regex.Matches(finalQuery, @"(?:FROM|JOIN)\s+(\w+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
                                .Cast<System.Text.RegularExpressions.Match>()
                                .Select(m => m.Groups[1].Value)
                                .Where(t => !new[] { "SELECT", "WHERE", "GROUP", "ORDER", "HAVING", "ON", "AS", "INNER", "LEFT", "RIGHT", "OUTER" }.Contains(t.ToUpper()))
                                .ToList();
                            
                            // If correction changed valid table names, revert to original
                            if (originalTables.Count == correctedTables.Count)
                            {
                                bool tablesChanged = !originalTables.SequenceEqual(correctedTables, StringComparer.OrdinalIgnoreCase);
                                if (tablesChanged)
                                {
                                    // Correction changed valid table names - use original
                                    finalQuery = sqlQuery;
                                    stepTitle = "Validating Query";
                                    stepContent = "Query is valid ✓";
                                }
                            }
                        }
                    }
                    
                    response.Steps.Add(new AIChatStep
                    {
                        StepNumber = 2,
                        Title = stepTitle,
                        Status = "completed",
                        Content = stepContent,
                        IsSQL = true,
                        SQLQuery = finalQuery
                    });
                    sqlQuery = finalQuery;

                    await Task.Delay(2000); // 2 second delay

                    // Step 3: Execute query and format response
                    try
                    {
                        // Execute raw SQL and get results as dynamic objects
                        var connection = _context.Database.GetDbConnection();
                        var wasOpen = connection.State == System.Data.ConnectionState.Open;
                        if (!wasOpen)
                        {
                            await connection.OpenAsync();
                        }
                        
                        var queryResult = new List<Dictionary<string, object>>();
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = sqlQuery;
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    var row = new Dictionary<string, object>();
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                        // Convert DateTime to string for JSON serialization
                                        var colName = reader.GetName(i) ?? $"Col{i}";
                                        if (value is DateTime dt)
                                        {
                                            row[colName] = dt.ToString("yyyy-MM-dd HH:mm:ss");
                                        }
                                        else
                                        {
                                            row[colName] = value;
                                        }
                                    }
                                    queryResult.Add(row);
                                }
                            }
                        }
                        
                        if (!wasOpen)
                        {
                            await connection.CloseAsync();
                        }
                        
                        // Format the response using AI to create a human-like answer
                        var queryResultsJson = System.Text.Json.JsonSerializer.Serialize(queryResult, new System.Text.Json.JsonSerializerOptions 
                        { 
                            WriteIndented = false 
                        });
                        
                        var humanResponsePrompt = $@"You are a helpful database assistant. The user asked a question, and a SQL query was executed to answer it.

USER'S QUESTION: {request.Message}

SQL QUERY EXECUTED:
{sqlQuery}

QUERY RESULTS (JSON):
{queryResultsJson}

TASK: Generate a natural, conversational response that directly answers the user's question using the query results data. 

RULES:
1. Answer in a friendly, human-like way (like a colleague explaining the data)
2. Include the actual numbers/values from the query results
3. Be concise and direct - don't explain the technical details
4. Use natural language - avoid phrases like 'the query returned' or 'the database shows'
5. If it's a count, say something like 'In [project name], we have [count] customers'
6. If it's a list, summarize it naturally
7. Make it sound like you're having a conversation, not reading from a report

EXAMPLE:
- If question is 'How many customers in Zahid Heights?' and result is {{'CustomerCount': 409}}
- Response should be: 'In Zahid Heights, we have 409 customers'

Generate ONLY the response text, nothing else.";

                        var humanResponse = await groqService.ChatAsync(humanResponsePrompt, "You are a friendly database assistant. Answer questions naturally using the query results, as if explaining to a colleague.");

                        response.Steps.Add(new AIChatStep
                        {
                            StepNumber = 3,
                            Title = "Executing Query & Formatting Response",
                            Status = "completed",
                            Content = humanResponse,
                            IsSQL = false,
                            Data = queryResult
                        });
                    }
                    catch (Exception ex)
                    {
                        response.Steps.Add(new AIChatStep
                        {
                            StepNumber = 3,
                            Title = "Query Execution",
                            Status = "error",
                            Content = $"Error executing query: {ex.Message}",
                            IsSQL = false
                        });
                    }
                }
                else
                {
                    // No SQL query needed - extract the response (remove NO_QUERY prefix if present)
                    var responseText = step1Result;
                    if (step1Trimmed.StartsWith("NO_QUERY", StringComparison.OrdinalIgnoreCase))
                    {
                        responseText = step1Trimmed.Substring("NO_QUERY".Length).Trim();
                        // Remove any leading/trailing punctuation or whitespace
                        responseText = responseText.TrimStart(':', '-', ' ').Trim();
                    }
                    
                    // Update step 1 content to show the response
                    response.Steps[0].Content = responseText;
                    response.Steps[0].Status = "completed";
                    response.Steps[0].IsSQL = false;
                    
                    // No need for step 2 or 3 for non-SQL questions
                }

                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(new AIChatResponse
                {
                    Steps = new List<AIChatStep>
                    {
                        new AIChatStep
                        {
                            StepNumber = 1,
                            Title = "Error",
                            Status = "error",
                            Content = $"An error occurred: {ex.Message}",
                            IsSQL = false
                        }
                    }
                });
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
