using Purchases.Application.Contracts;
using Purchases.Application.Models.Settings;
using Purchases.Application.Repository;
using Purchases.Application.Services;

namespace aspnet_mongo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var allowedOriginsPolicyName = "_allowedOrigins";

            var builder = WebApplication.CreateBuilder(args);

            // Add CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: allowedOriginsPolicyName,
                policy =>
                {
                    // TODO: Re-enable specific origins once able to fix this
                    // policy.WithOrigins(@"https://aspnet-mongo.azurewebsites.net");
                    policy.AllowAnyHeader();
                    policy.AllowAnyMethod();
                    policy.AllowAnyOrigin();
                });
            });

            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Dependency Injection Setup
            builder.Services.AddScoped<IPurchaseService, PurchaseService>();
            builder.Services.AddScoped<IVendorService, VendorService>();
            builder.Services.AddScoped<IReceiptRetrieverService, ReceiptRetrieverService>();
            builder.Services.AddScoped<IPurchasesRepository, PurchasesRepository>();

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
            app.UseCors(allowedOriginsPolicyName);
            app.UseAuthorization();

            app.Run();
        }
    }
}