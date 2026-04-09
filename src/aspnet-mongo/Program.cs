using Purchases.Application.Repository;
using Purchases.Application.Services;
using Purchases.Domain.Contracts.Repos;
using Purchases.Domain.Contracts.Services;
using Purchases.Domain.Models.Settings;
using Purchases.Infrastructure.Repository;

namespace aspnet_mongo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var allowReactAppCorsName = "AllowReactApp";

            var builder = WebApplication.CreateBuilder(args);

            // Add CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: allowReactAppCorsName,
                policy =>
                {
                    policy
                        .WithOrigins(
                            "https://react-purchases.vercel.app"
                            //"http://localhost:5173" // In case testing FE locally
                            )
                        .AllowCredentials();

                    policy.AllowAnyHeader();
                    policy.AllowAnyMethod();
                });
            });

            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

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

            builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("PurchasesDatabase"));
            builder.Services.Configure<TelegramIntegrationSettings>(builder.Configuration.GetSection("TelegramIntegration"));
            builder.Services.Configure<OpenAiSettings>(builder.Configuration.GetSection("OpenAiIntegration"));

            // Generic Scraper Client Setup
            builder.Services.AddHttpClient("Scraper", client =>
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                client.Timeout = TimeSpan.FromSeconds(20);
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();
            app.MapControllers();
            app.UseRouting();
            app.UseCors(allowReactAppCorsName);
            app.UseAuthorization();

            app.Run();
        }
    }
}