using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Seek.API.Services.Interceptors;
using Seek.EF;

namespace Seek.API.Security.New
{
    /// <summary>
    /// Extension methods for the ApplicationDbContext to add security features
    /// </summary>
    public static class ApplicationDbContextExtensions
    {
        /// <summary>
        /// Creates a configured DbContextOptionsBuilder with proper encryption settings
        /// </summary>
        public static DbContextOptionsBuilder ConfigureEncryption(
            this DbContextOptionsBuilder options,
            string connectionString,
            IServiceProvider serviceProvider)
        {
            options.UseSqlite(connectionString, sqliteOptions =>
            {
                sqliteOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                sqliteOptions.CommandTimeout(600);
            });

            // Add encryption interceptor
            var interceptor = serviceProvider.GetRequiredService<SqliteEncryptionInterceptor>();
            options.AddInterceptors(interceptor);

            return options;
        }

        /// <summary>
        /// Checks if the application is verified and the database is accessible
        /// </summary>
        public static async Task<bool> IsDatabaseAccessible(
            this IServiceProvider serviceProvider,
            ILogger logger)
        {
            try
            {
                // Check if the application is verified
                var verificationService = serviceProvider.GetRequiredService<VerificationService>();

                if (!verificationService.IsVerificationCompleted())
                {
                    logger.LogWarning("Application is not verified. Database access denied.");
                    return false;
                }

                // Check database access
                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Try a simple query
                await dbContext.Database.ExecuteSqlRawAsync("SELECT 1");

                logger.LogInformation("Database is accessible and verified");
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to access database");
                return false;
            }
        }
    
}
}