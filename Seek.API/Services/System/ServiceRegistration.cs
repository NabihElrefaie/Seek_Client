using Seek.API.Security.New;
using Seek.Core.Dtos.Settings;
using Seek.Core.Helper_Classes;
using Seek.Core.IRepositories.Database;
using Seek.Core.IRepositories.System;
using Seek.Core.Security;
using Seek.EF.Repositories.Database;
using Seek.EF.Repositories.System;

namespace Seek.API.Services.System
{
    public static class ServiceRegistration
    {
        public static void AddRepositoryServices(this IServiceCollection services, IConfiguration configuration)
        {

            //Services
            services.AddHttpContextAccessor();
            services.AddHttpClient();
            // Add database encryption services
            services.AddDatabaseEncryptionServices();
            // Configure Email Service
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.AddSingleton<EmailService>();
            // Repositories and Interfaces Injection
            services.AddScoped<IRepo_Database_Security, Repo_Database_Security>();
            services.AddScoped<IRepo_Database_Existence_Checker, Repo_Database_Existence_Checker>();
            services.AddSingleton<IRepo_SecureKeyManager, Repo_SecureKeyManager>();

        }
    }
}
