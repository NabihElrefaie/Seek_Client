using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.EF.Repositories.Services
{
    public class MaintenanceService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public MaintenanceService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public void PauseServices()
        {
            // Dispose all DbContexts (or services using the DB)
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Dispose();
            }
        }

        public void ResumeServices()
        {
            // You can add any code here to restart necessary services or DbContexts.
        }
    }
}
