using Purchases.Domain.Contracts.Services;

namespace Purchases.Worker
{
    public class Worker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IPurchaseService _purchaseService;
        private readonly IReceiptRetrieverService _receiptRetrieverService;
        private readonly ILogger<Worker> _logger;

        public Worker(
            IServiceScopeFactory scopeFactory,
            // IPurchaseService purchaseService,
            // IReceiptRetrieverService receiptRetrieverService,
            ILogger<Worker> logger)
        {
            using IServiceScope scope = scopeFactory.CreateScope();

            _purchaseService = scope.ServiceProvider.GetRequiredService<IPurchaseService>();
            _receiptRetrieverService = scope.ServiceProvider.GetRequiredService<IReceiptRetrieverService>();
            _logger = scope.ServiceProvider.GetRequiredService<ILogger<Worker>>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                    try
                    {
                        // Get 1 record
                        var purchase = await _purchaseService.GetAsync("655bc864a924696403ac1d45", stoppingToken);

                        if (purchase == null)
                            return;

                        var url = purchase.PurchaseUrl;
                        var retrievedReceipt = await _receiptRetrieverService.HandleReceiptUrl(url, default, stoppingToken);

                    }
                    catch (Exception exc)
                    {
                        _logger.LogError(exc, $"Error executing worker: {exc.Message}");
                    }


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
