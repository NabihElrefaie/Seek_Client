using Seek.Core.Helper_Classes;

namespace Seek.API.Services.Interceptors
{
    public class BackgroundJobService : IHostedService
    {
        private readonly MaintenanceService _maintenanceService;
        private readonly ILogger<BackgroundJobService> _logger;
        private const int MaxRetries = 10;

        public BackgroundJobService(MaintenanceService maintenanceService, ILogger<BackgroundJobService> logger)
        {
            _maintenanceService = maintenanceService;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("System : Background job service started.");

            // Continuously check if we are in maintenance mode
            Task.Run(async () =>
            {
                int retryCount = 0;

                while (!cancellationToken.IsCancellationRequested)
                {
                    if (_maintenanceService.IsInMaintenance)
                    {
                        // In maintenance mode, retry after delay
                        _logger.LogInformation("System : Background job paused due to maintenance mode.");
                        await Task.Delay(5000);
                        retryCount++;

                        if (retryCount >= MaxRetries)
                        {
                            _logger.LogError("System : Max retry attempts reached. Job will stop.");
                            break;
                        }
                    }
                    else
                    {
                        // Proceed with your job's logic here
                        _logger.LogInformation("System : Running background job...");
                        await DoBackgroundJobWorkAsync();
                        retryCount = 0;
                        await Task.Delay(10000); // Check every 10 seconds (or whatever your job interval is)
                    }
                }
            });

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("System : Background job service stopped.");
            return Task.CompletedTask;
        }

        private async Task DoBackgroundJobWorkAsync()
        {
            // Background job logic here
            await Task.Delay(2000); // Simulate job work
        }
    }

}
