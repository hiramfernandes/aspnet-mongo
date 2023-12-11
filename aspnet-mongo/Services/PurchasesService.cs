using aspnet_mongo.Models;
using aspnet_mongo.Models.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace aspnet_mongo.Services
{
    public interface IPurchasesService
    {
        Task CreateAsync(Purchase newPurchase, CancellationToken cancellationToken);
        Task<List<Purchase>> GetAllAsync();
        Task<Purchase?> GetAsync(string id);
        Task RemoveAsync(string id);
        Task UpdateAsync(string id, Purchase updatedPurchase);
    }

    public class PurchasesService : IPurchasesService
    {
        private readonly IMongoCollection<Purchase> _purchasesCollection;

        public PurchasesService(IOptions<MongoDbSettings> databaseSettings)
        {
            var connectionString = databaseSettings.Value.ConnectionString;
            var collectionName = databaseSettings.Value.CollectionName;
            var dbName = databaseSettings.Value.DatabaseName;

            var mongoClient = new MongoClient(connectionString);
            var mongoDatabase = mongoClient.GetDatabase(dbName);

            _purchasesCollection = mongoDatabase.GetCollection<Purchase>(collectionName);
        }

        public async Task<List<Purchase>> GetAllAsync()
        {
            var queryableCollection = _purchasesCollection.AsQueryable();
            return await Task.FromResult( queryableCollection.OrderByDescending(x => x.PurchaseDate).ToList() );
        }

        public async Task<Purchase?> GetAsync(string id) =>
            await _purchasesCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Purchase newPurchase, CancellationToken cancellationToken) =>
            await _purchasesCollection.InsertOneAsync(newPurchase, null, cancellationToken);

        public async Task UpdateAsync(string id, Purchase updatedPurchase) =>
            await _purchasesCollection.ReplaceOneAsync(x => x.Id == id, updatedPurchase);

        public async Task RemoveAsync(string id) =>
            await _purchasesCollection.DeleteOneAsync(x => x.Id == id);
    }
}

