using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using MongoDB.Driver;
using Purchases.Application.Services;
using Purchases.Domain.Contracts.Repos;
using Purchases.Domain.Contracts.Services;
using Purchases.Domain.Models.Settings;
using Purchases.Infrastructure.Repository;
using System.Text;

namespace aspnet_mongo;

public class Program
{
    public static void Main(string[] args)
    {
        const string schemeId = "bearer";
        const string allowReactAppCorsName = "AllowReactApp";

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
        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JWT"));

        builder.Services.AddSingleton<IMongoClient>(sp =>
        {
            var mongoDbSection = builder.Configuration.GetSection("PurchasesDatabase");
            var connString = mongoDbSection.GetValue<string>("ConnectionString") ?? throw new InvalidOperationException("Unable to retrieve Mongo db settings");

            return new MongoClient(connString);
        });

        // Add JWT Authentication
        var jwtSettings = builder.Configuration.GetSection("JWT").Get<JwtSettings>();

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var jwtIssuer = jwtSettings?.Issuer;
                var jwtAudience = jwtSettings?.Audience;
                var jwtKey = jwtSettings?.Key ?? throw new InvalidOperationException("Invalid Key");

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ClockSkew = TimeSpan.FromDays(1)
                };
            });

        builder.Services.AddAuthorization();

        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Purchases API", Version = "v1" });
            options.AddSecurityDefinition(schemeId, new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                //In = ParameterLocation.Header,
                //Name = "Authorization",
                Description = "JWT Authorization Token",
            });

            options.AddSecurityRequirement(document =>
            {
                return new()
                {
                    [
                        new OpenApiSecuritySchemeReference(schemeId)
                        {
                            Reference = new OpenApiReferenceWithDescription
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = schemeId
                            }
                        }
                    ] = []
                };
            });
        });



        // Generic Scraper Client Setup
        builder.Services.AddHttpClient("Scraper", client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            client.Timeout = TimeSpan.FromSeconds(20);
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.MapControllers();
        app.UseRouting();
        app.UseCors(allowReactAppCorsName);
        app.UseAuthentication();
        app.UseAuthorization();

        app.Run();
    }
}