using aspnet_mongo.Models.Settings;
using aspnet_mongo.Services;

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
                    policy.WithOrigins("https://aspnet-mongo.azurewebsites.net/");
                });
            });

            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddScoped<IPurchasesService, PurchasesService>();
            builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("PurchasesDatabase"));

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