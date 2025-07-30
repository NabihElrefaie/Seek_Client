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

        }
    }
}
