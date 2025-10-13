using Microsoft.EntityFrameworkCore;
using PMS.Data;
using PMS.Models;
using BCrypt.Net;

namespace PMS.Services
{
    public class SeedDataService
    {
        private readonly PMSDbContext _context;

        public SeedDataService(PMSDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            // Ensure database is created
            await _context.Database.EnsureCreatedAsync();

            // Seed ACL Roles
            await SeedRoles();

            // Seed Admin User
            await SeedAdminUser();

            // Seed Sample Projects
            await SeedProjects();

            // Seed Sample Properties
            await SeedProperties();

            // Seed Sample Payment Plans
            await SeedPaymentPlans();
        }

        private async Task SeedRoles()
        {
            if (!await _context.ACLs.AnyAsync())
            {
                var roles = new List<ACL>
                {
                    new ACL { RoleID = "ADMIN001", RoleName = "Admin", Permissions = "All" },
                    new ACL { RoleID = "MANAGER01", RoleName = "Manager", Permissions = "Customers,Properties,Payments" },
                    new ACL { RoleID = "STAFF001", RoleName = "Staff", Permissions = "Customers,Properties" }
                };

                _context.ACLs.AddRange(roles);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedAdminUser()
        {
            if (!await _context.Users.AnyAsync(u => u.Email == "admin@jubasmartcity.com"))
            {
                var adminUser = new User
                {
                    UserID = "USER00001",
                    FullName = "System Administrator",
                    Email = "admin@jubasmartcity.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    RoleID = "ADMIN001",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(adminUser);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedProjects()
        {
            if (!await _context.Projects.AnyAsync())
            {
                var projects = new List<Project>
                {
                    new Project
                    {
                        ProjectID = "PROJ00001",
                        ProjectName = "Juba Smart City Phase 1",
                        Type = "Residential",
                        Location = "Juba, South Sudan",
                        Description = "First phase of Juba Smart City development with modern residential units",
                        CreatedAt = DateTime.Now
                    },
                    new Project
                    {
                        ProjectID = "PROJ00002",
                        ProjectName = "Juba Smart City Phase 2",
                        Type = "Commercial",
                        Location = "Juba, South Sudan",
                        Description = "Commercial phase with office buildings and shopping centers",
                        CreatedAt = DateTime.Now
                    },
                    new Project
                    {
                        ProjectID = "PROJ00003",
                        ProjectName = "Juba Smart City Phase 3",
                        Type = "Mixed",
                        Location = "Juba, South Sudan",
                        Description = "Mixed development with residential and commercial units",
                        CreatedAt = DateTime.Now
                    }
                };

                _context.Projects.AddRange(projects);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedProperties()
        {
            if (!await _context.Properties.AnyAsync())
            {
                var properties = new List<Property>();

                // Generate properties for each project
                for (int i = 1; i <= 3; i++)
                {
                    var projectId = $"PROJ0000{i}";
                    
                    for (int j = 1; j <= 20; j++)
                    {
                        properties.Add(new Property
                        {
                            PropertyID = $"PROP{i:D3}{j:D3}",
                            ProjectID = projectId,
                            PlotNo = $"{i}{j:D3}",
                            Street = $"Street {j}",
                            PlotType = j % 2 == 0 ? "Residential" : "Commercial",
                            Block = $"Block {((j - 1) / 5) + 1}",
                            PropertyType = j % 3 == 0 ? "Apartment" : "Plot",
                            Size = j % 2 == 0 ? "1000 sq ft" : "1500 sq ft",
                            Status = j <= 5 ? "Allotted" : "Available",
                            CreatedAt = DateTime.Now,
                            AdditionalInfo = $"Property in Juba Smart City Phase {i}"
                        });
                    }
                }

                _context.Properties.AddRange(properties);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedPaymentPlans()
        {
            if (!await _context.PaymentPlans.AnyAsync())
            {
                var paymentPlans = new List<PaymentPlan>
                {
                    new PaymentPlan
                    {
                        PlanID = "PLAN00001",
                        ProjectID = "PROJ00001",
                        PlanName = "Basic Residential Plan",
                        TotalAmount = 50000,
                        DurationMonths = 36,
                        Frequency = "Monthly",
                        Description = "Basic payment plan for residential properties",
                        CreatedAt = DateTime.Now
                    },
                    new PaymentPlan
                    {
                        PlanID = "PLAN00002",
                        ProjectID = "PROJ00001",
                        PlanName = "Premium Residential Plan",
                        TotalAmount = 75000,
                        DurationMonths = 48,
                        Frequency = "Monthly",
                        Description = "Premium payment plan for residential properties",
                        CreatedAt = DateTime.Now
                    },
                    new PaymentPlan
                    {
                        PlanID = "PLAN00003",
                        ProjectID = "PROJ00002",
                        PlanName = "Commercial Plan",
                        TotalAmount = 100000,
                        DurationMonths = 60,
                        Frequency = "Quarterly",
                        Description = "Payment plan for commercial properties",
                        CreatedAt = DateTime.Now
                    }
                };

                _context.PaymentPlans.AddRange(paymentPlans);
                await _context.SaveChangesAsync();

                // Seed payment schedules
                await SeedPaymentSchedules();
            }
        }

        private async Task SeedPaymentSchedules()
        {
            var paymentPlans = await _context.PaymentPlans.ToListAsync();

            foreach (var plan in paymentPlans)
            {
                var schedules = new List<PaymentSchedule>();
                var monthlyAmount = plan.TotalAmount / plan.DurationMonths;

                for (int i = 1; i <= plan.DurationMonths; i++)
                {
                    schedules.Add(new PaymentSchedule
                    {
                        ScheduleID = $"SCH{plan.PlanID.Substring(4)}{i:D3}",
                        PlanID = plan.PlanID,
                        PaymentDescription = $"Installment {i} of {plan.DurationMonths}",
                        InstallmentNo = i,
                        DueDate = DateTime.Now.AddMonths(i),
                        Amount = monthlyAmount,
                        SurchargeApplied = true,
                        SurchargeRate = 0.05m,
                        Description = $"Monthly installment for {plan.PlanName}"
                    });
                }

                _context.PaymentSchedules.AddRange(schedules);
            }

            await _context.SaveChangesAsync();
        }
    }
}
