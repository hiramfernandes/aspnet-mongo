namespace Purchases.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                    // TODO: Obtain existing receipt records
                    // Get URL
                    // Send to LLM for Info retrieval
                    // Compare to existing records or just override with fresh info

                    // Edge cases:
                    // - Look for duplicates
                    // - Look for inconsistent data (manual input vs LLM reported)
                    // - Save items into a dedicated container??

                }
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
