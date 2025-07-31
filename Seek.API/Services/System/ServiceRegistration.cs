using Seek.Core.Helper_Classes;
using Seek.Core.IRepositories;
using Seek.EF.Repositories;

namespace Seek.API.Services.System
{
    public static class ServiceRegistration
    {
        public static void AddRepositoryServices(this IServiceCollection services)
        {
            //Services
            services.AddHttpContextAccessor();
            services.AddHttpClient();
            services.AddSingleton<MaintenanceService>();
            // Repositories and Interfaces Injection
            services.AddScoped<IRepo_Database_Security, Repo_Database_Security>();

        }
    }
}
