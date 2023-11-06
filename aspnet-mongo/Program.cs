using aspnet_mongo.Models.Settings;
using aspnet_mongo.Services;

namespace aspnet_mongo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddScoped<IPurchasesService, PurchasesService>();
            builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("PurchasesDatabase"));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }


            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.UseRouting();

            //app.UseAuthorization();

            //app.MapRazorPages();

            app.Run();
        }
    }
}