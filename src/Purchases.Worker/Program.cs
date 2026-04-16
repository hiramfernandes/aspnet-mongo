using MongoDB.Driver;
using Purchases.Application.Services;
using Purchases.Domain.Contracts.Repos;
using Purchases.Domain.Contracts.Services;
using Purchases.Domain.Models.Settings;
using Purchases.Infrastructure.Repository;

namespace Purchases.Worker;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<Worker>();

        builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("PurchasesDatabase"));
        builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("PurchasesDatabase"));
        builder.Services.Configure<TelegramIntegrationSettings>(builder.Configuration.GetSection("TelegramIntegration"));
        builder.Services.Configure<OpenAiSettings>(builder.Configuration.GetSection("OpenAiIntegration"));

        // Dependency Injection Setup for Services
        builder.Services.AddScoped<IPurchaseService, PurchaseService>();
        builder.Services.AddScoped<IVendorService, VendorService>();
        builder.Services.AddScoped<IReceiptRetrieverService, ReceiptRetrieverService>();
        builder.Services.AddScoped<IMessageNotifier, TelegramMessageNotifier>();
        builder.Services.AddScoped<IRemoteFileManager, TelegramRemoteFileManager>();
        builder.Services.AddScoped<ILlmProcessor, LlmProcessor>();

        // DI for Repos
        builder.Services.AddScoped<IPurchaseRepository, PurchaseRepository>();
        builder.Services.AddScoped<IVendorRepository, VendorRepository>();

        builder.Services.AddSingleton<IMongoClient>(sp =>
        {
            var mongoDbSection = builder.Configuration.GetSection("PurchasesDatabase");
            var connString = mongoDbSection.GetValue<string>("ConnectionString") ?? throw new InvalidOperationException("Unable to retrieve Mongo db settings");

            return new MongoClient(connString);
        });

        builder.Services.AddHttpClient("Scraper", client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            client.Timeout = TimeSpan.FromSeconds(20);
        });

        var host = builder.Build();
        host.Run();
    }
}