using aspnet_mongo.Models;
using aspnet_mongo.Models.DTO;
using aspnet_mongo.Models.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace aspnet_mongo.Services
{
    public interface IPurchaseService
    {
        Task CreateAsync(Purchase newPurchase);
        Task<IEnumerable<GetPurchaseDto>> GetAllAsync(int pageSize = 50);
        Task<Purchase?> GetAsync(string id);
        Task RemoveAsync(string id);
        Task UpdateAsync(string id, Purchase updatedPurchase);
    }

    public class PurchaseService : IPurchaseService
    {
        private readonly string _collectionName = "purchases";

        private readonly IMongoCollection<Purchase> _purchasesCollection;
        private readonly IVendorService _vendorService;

        public PurchaseService(IOptions<MongoDbSettings> databaseSettings, IVendorService vendorService)
        {
            var connectionString = databaseSettings.Value.ConnectionString;
            var collectionName = databaseSettings.Value.CollectionName;
            var dbName = databaseSettings.Value.DatabaseName;

            var mongoClient = new MongoClient(connectionString);
            var mongoDatabase = mongoClient.GetDatabase(dbName);

            _vendorService = vendorService;
            _purchasesCollection = mongoDatabase.GetCollection<Purchase>(collectionName);
        }

        public async Task<IEnumerable<GetPurchaseDto>> GetAllAsync(int pageSize = 50)
        {
            var queryableCollection = _purchasesCollection.AsQueryable();
            var purchases = queryableCollection
                .OrderByDescending(x => x.PurchaseDate)
                .Take(pageSize)
                .ToList();

            var vendors = await _vendorService.GetAllAsync();
            var purchaseDtos = purchases.Select(purchase => MapFrom(purchase, vendors));

            return purchaseDtos;
        }

        public async Task<Purchase?> GetAsync(string id) =>
            await _purchasesCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Purchase newPurchase) =>
            await _purchasesCollection.InsertOneAsync(newPurchase);

        public async Task UpdateAsync(string id, Purchase updatedPurchase) =>
            await _purchasesCollection.ReplaceOneAsync(x => x.Id == id, updatedPurchase);

        public async Task RemoveAsync(string id) =>
            await _purchasesCollection.DeleteOneAsync(x => x.Id == id);

        private GetPurchaseDto MapFrom(Purchase purchase, IEnumerable<GetVendorDto> vendors)
        {
            var vendor = vendors.FirstOrDefault(x => x.Id == purchase.VendorId);
            return new GetPurchaseDto()
            {
                Id = purchase.Id,
                PurchaseDate = purchase.PurchaseDate,
                PurchaseUrl = purchase.PurchaseUrl,
                TotalAmount = purchase.TotalAmount,
                VendorId = vendor?.Id,
                VendorName = vendor?.Name,
                VendorLogoUrl = vendor?.LogoUrl
            };
        }
    }
}

