using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace PMS.Services
{
    public class SiteConfig
    {
        public string? ProjectName { get; set; } = "PMS";
        public string? Tagline { get; set; } = "Property Management System";
        public string? WebsiteUrl { get; set; } = "";
        public string? Email { get; set; } = "";
        public string LogoPath { get; set; } = "~/images/PMS.png";
    }

    public interface ISiteConfigService
    {
        Task<SiteConfig> GetAsync();
        Task SaveAsync(SiteConfig config);
    }

    public class SiteConfigService : ISiteConfigService
    {
        private readonly IWebHostEnvironment _env;
        private readonly string _configPath;

        public SiteConfigService(IWebHostEnvironment env)
        {
            _env = env;
            var root = _env.ContentRootPath ?? AppContext.BaseDirectory;
            var configDir = Path.Combine(root, "App_Data");
            if (!Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }
            _configPath = Path.Combine(configDir, "siteconfig.json");
        }

        public async Task<SiteConfig> GetAsync()
        {
            if (!File.Exists(_configPath))
            {
                return new SiteConfig();
            }

            try
            {
                var json = await File.ReadAllTextAsync(_configPath);
                var cfg = JsonSerializer.Deserialize<SiteConfig>(json);
                return cfg ?? new SiteConfig();
            }
            catch
            {
                return new SiteConfig();
            }
        }

        public async Task SaveAsync(SiteConfig config)
        {
            // #region agent log
            try
            {
                var logPayload = new
                {
                    sessionId = "78c481",
                    runId = "pre-fix",
                    hypothesisId = "H3",
                    location = "SiteConfigService.SaveAsync",
                    message = "Saving SiteConfig",
                    data = new
                    {
                        config.ProjectName,
                        config.Tagline,
                        config.WebsiteUrl,
                        config.Email,
                        config.LogoPath
                    },
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };
                var logJson = JsonSerializer.Serialize(logPayload);
                var logPath = @"C:\Users\User\.cursor\projects\d-PMS-PMS-PMS\debug-78c481.log";
                File.AppendAllText(logPath, logJson + Environment.NewLine);
            }
            catch
            {
                // Swallow logging errors
            }
            // #endregion

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(config, options);
            await File.WriteAllTextAsync(_configPath, json);
        }
    }
}

