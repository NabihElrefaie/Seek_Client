using Seek.Core.Dtos.Settings;
using Seek.Core.Security;

namespace Seek.API.Security.New
{
    /// <summary>
    /// Extensions for configuring security services in application startup
    /// </summary>
    public static class StartupExtensions
    {
        /// <summary>
        /// Adds all security-related services to the service collection
        /// </summary>
        public static IServiceCollection AddSeekSecurityServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add database encryption services
            services.AddDatabaseEncryptionServices();

            // Configure Email Service
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.AddSingleton<EmailService>();

            return services;
        }

        /// <summary>
        /// Configures security middleware for the application
        /// </summary>
        public static IApplicationBuilder UseSeekSecurity(this IApplicationBuilder app)
        {
            // Add verification check middleware
            app.UseVerificationCheck();

            return app;
        }

        /// <summary>
        /// Initializes secure settings by transferring from appsettings to encrypted storage
        /// </summary>
        public static void InitializeSecureSettings(this IApplicationBuilder app)
        {
            // Get services
            var settingsManager = app.ApplicationServices.GetRequiredService<SecureSettingsManager>();
            var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();

            // Check if secure settings already exist
            var emailSettings = settingsManager.GetSecureEmailSettings();

            // If no secure settings exist, copy from configuration
            if (emailSettings == null || string.IsNullOrEmpty(emailSettings.SmtpServer))
            {
                emailSettings = configuration.GetSection("EmailSettings").Get<EmailSettings>();
                if (emailSettings != null && !string.IsNullOrEmpty(emailSettings.SmtpServer))
                {
                    settingsManager.SaveSecureEmailSettings(emailSettings);
                }
            }
        }
    }
}