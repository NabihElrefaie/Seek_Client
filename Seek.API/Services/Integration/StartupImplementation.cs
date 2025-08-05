using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Seek.API.Security;
using System;
using Seek.API.Security.New;
using Seek.Core.Security;
using Seek.API.Services.Interceptors;

namespace Seek.API.Services.Integration
{
    /// <summary>
    /// Example implementation of the security features in the StartUp class
    /// </summary>
    public class StartupSecurityExample
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public StartupSecurityExample(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Add all security services
            services.AddSeekSecurityServices(_configuration);

            // Configure database encryption
            ConfigureDatabaseWithEncryption(services);

            // Other services...
            services.AddControllers();
        }

        private void ConfigureDatabaseWithEncryption(IServiceCollection services)
        {
            // Register SecureKeyManager (if not already registered by AddSeekSecurityServices)
            if (services.BuildServiceProvider().GetService<SecureKeyManager>() == null)
            {
                services.AddSingleton<SecureKeyManager>(provider =>
                    new SecureKeyManager(
                        Directory.GetCurrentDirectory(),
                        provider.GetRequiredService<ILogger<SecureKeyManager>>(),
                        provider.GetService<EmailService>()
                    )
                );
            }

            // Register VerificationService (if not already registered)
            if (services.BuildServiceProvider().GetService<VerificationService>() == null)
            {
                services.AddSingleton<VerificationService>(provider =>
                    new VerificationService(
                        Directory.GetCurrentDirectory(),
                        provider.GetRequiredService<ILogger<VerificationService>>()
                    )
                );
            }

            // Register enhanced encryption interceptor
            services.AddSingleton<SqliteEncryptionInterceptor>(provider =>
                provider.CreateWithVerification(_configuration["EncryptionKey"])
            );
        }

        public void Configure(IApplicationBuilder app)
        {
            // Initialize secure settings (move email settings to encrypted storage)
            app.InitializeSecureSettings();

            // Add verification middleware
            app.UseSeekSecurity();

            // Other middleware...
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
