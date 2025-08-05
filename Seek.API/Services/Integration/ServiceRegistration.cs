using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Seek.API.Controllers.System;
using Seek.API.Security.New;

namespace Seek.API.Services.Integration
{
    /// <summary>
    /// Helper class for registering all security services
    /// </summary>
    public static class SecurityServiceRegistration
    {
        /// <summary>
        /// Register all services required for the security implementation
        /// </summary>
        public static IServiceCollection RegisterSecurityServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register core security services
            services.AddSeekSecurityServices(configuration);

            // Register controllers
            services.AddControllers()
                .AddApplicationPart(typeof(VerificationController).Assembly)
                .AddApplicationPart(typeof(SecureSettingsController).Assembly);

            // Register repository integrations
            services.AddScoped<Seek.Core.IRepositories.Database.IRepo_Database_Security, Seek.EF.Repositories.Database.Repo_Database_Security>();

            return services;
        }

        /// <summary>
        /// Register services for simpler integration tests
        /// </summary>
        public static IServiceCollection RegisterSecurityServicesForTesting(this IServiceCollection services)
        {
            // Register with in-memory configuration
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection()
                .Build();

            // Set minimum required configuration
            configuration["EncryptionKey"] = "TestEncryptionKey123!";
            configuration["EmailSettings:SmtpServer"] = "smtp.gmail.com";
            configuration["EmailSettings:SmtpPort"] = "587";
            configuration["EmailSettings:Username"] = "ti.tickets.23@gmail.com";
            configuration["EmailSettings:Password"] = "fblzdcgxntglxzcx";
            configuration["EmailSettings:FromEmail"] = "ti.tickets.23@gmail.com";
            configuration["EmailSettings:AdminEmail"] = "Nabihabdelkhalek6@gmail.com";

            // Register services with test configuration
            return services.RegisterSecurityServices(configuration);
        }
    }
}
