using PMS.Data;
using Microsoft.EntityFrameworkCore;
using PMS.Services;

namespace PMS.Services
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PMSDbContext>();
            var seedService = scope.ServiceProvider.GetRequiredService<SeedDataService>();

            try
            {
                // Apply pending migrations and create database if it doesn't exist
                context.Database.Migrate();

                // Seed initial data
                await seedService.SeedAsync();
            }
            catch (Exception ex)
            {
                // Log error or handle as needed
                Console.WriteLine($"Database initialization error: {ex.Message}");
            }
        }
    }
}
