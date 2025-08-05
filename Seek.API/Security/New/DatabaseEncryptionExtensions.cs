using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Seek.Core.IRepositories.Database;
using Seek.EF;
using Microsoft.EntityFrameworkCore;
using Seek.Core.Security;

namespace Seek.API.Security.New
{
    /// <summary>
    /// Extensions for integrating database encryption with the application startup
    /// </summary>
    public static class DatabaseEncryptionExtensions
    {
        /// <summary>
        /// Registers database encryption services
        /// </summary>
        public static IServiceCollection AddDatabaseEncryptionServices(this IServiceCollection services)
        {
            // Register SecureKeyManager if not already registered
            services.AddSingleton(provider =>
                new SecureKeyManager(
                    Directory.GetCurrentDirectory(),
                    provider.GetRequiredService<ILogger<SecureKeyManager>>(),
                    provider.GetService<EmailService>()
                )
            );

            // Register VerificationService
            services.AddSingleton(provider =>
                new VerificationService(
                    Directory.GetCurrentDirectory(),
                    provider.GetRequiredService<ILogger<VerificationService>>()
                )
            );

            // Register SecureSettingsManager
            services.AddSingleton(provider =>
                new SecureSettingsManager(
                    Directory.GetCurrentDirectory(),
                    provider.GetRequiredService<ILogger<SecureSettingsManager>>(),
                    provider.GetRequiredService<IConfiguration>(),
                    provider.GetRequiredService<SecureKeyManager>()
                )
            );

            return services;
        }

        /// <summary>
        /// Configures application startup to check for verification before allowing database access
        /// </summary>
        public static IApplicationBuilder UseVerificationCheck(this IApplicationBuilder app)
        {
            var verificationService = app.ApplicationServices.GetService<VerificationService>();

            if (verificationService == null)
            {
                throw new InvalidOperationException("VerificationService is not registered. Ensure AddDatabaseEncryptionServices has been called.");
            }

            return app.Use(async (context, next) =>
            {
                // Skip verification check for verification-related endpoints
                string path = context.Request.Path.Value?.ToLower() ?? "";

                if (path.Contains("/api/v1/verification") ||
                    path.Contains("/swagger") ||
                    path.Contains("/health"))
                {
                    await next();
                    return;
                }

                // Check verification status
                bool isVerified = verificationService.IsVerificationCompleted();

                if (!isVerified)
                {
                    // Redirect to verification status endpoint
                    context.Response.StatusCode = 403; // Forbidden
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"message\":\"Application requires verification\",\"verificationRequired\":true}");
                    return;
                }

                await next();
            });
        }

        /// <summary>
        /// Helper method to verify encrypted database connection
        /// </summary>
        public static async Task<bool> VerifyDatabaseEncryptionAsync(this ApplicationDbContext context, IRepo_Database_Security databaseSecurity, string encryptionKey)
        {
            try
            {
                // Try to execute a simple query to verify connection
                var connection = context.Database.GetDbConnection() as SqliteConnection;

                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                // Use the encryption key with PRAGMA
                using var command = connection.CreateCommand();
                command.CommandText = $"PRAGMA key = '{encryptionKey}';";
                await command.ExecuteNonQueryAsync();

                // Test query to verify encryption
                command.CommandText = "SELECT count(*) FROM sqlite_master;";
                await command.ExecuteScalarAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}