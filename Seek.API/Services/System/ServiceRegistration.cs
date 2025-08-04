using Seek.Core.Helper_Classes;
using Seek.Core.IRepositories.Database;
using Seek.EF.Repositories.Database;

namespace Seek.API.Services.System
{
    public static class ServiceRegistration
    {
        public static void AddRepositoryServices(this IServiceCollection services)
        {
            //Services
            services.AddHttpContextAccessor();
            services.AddHttpClient();
            // Repositories and Interfaces Injection
            services.AddScoped<IRepo_Database_Security, Repo_Database_Security>();
            services.AddScoped<IRepo_Database_Existence_Checker, Repo_Database_Existence_Checker>();

        }
    }
}
