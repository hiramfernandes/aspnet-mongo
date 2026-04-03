using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Purchases.Application.Models;
using Purchases.Application.Models.Settings;

namespace Purchases.Application.Repository
{
    public interface IPurchasesRepository
    {
        Task<IEnumerable<Purchase>> GetAllAsync(int pageSize = 50);
        Task<Purchase?> GetAsync(string id);
        Task CreateAsync(Purchase newPurchase);
        Task RemoveAsync(string id);
        Task UpdateAsync(string id, Purchase updatedPurchase);
    }

    public class PurchasesRepository : IPurchasesRepository
    {
        private readonly IMongoCollection<Purchase> _purchasesCollection;

        private readonly string _collectionName = "purchases";

        public PurchasesRepository(IOptions<MongoDbSettings> databaseSettings)
        {
            var connectionString = databaseSettings.Value.ConnectionString;
            var dbName = databaseSettings.Value.DatabaseName;

            var mongoClient = new MongoClient(connectionString);
            var mongoDatabase = mongoClient.GetDatabase(dbName);

            _purchasesCollection = mongoDatabase.GetCollection<Purchase>(_collectionName);
        }

        public async Task<IEnumerable<Purchase>> GetAllAsync(int pageSize = 50)
        {
            var queryableCollection = _purchasesCollection.AsQueryable();
            return await queryableCollection
                .OrderByDescending(x => x.PurchaseDate)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Purchase?> GetAsync(string id) =>
            await _purchasesCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Purchase newPurchase) =>
            await _purchasesCollection.InsertOneAsync(newPurchase);

        public async Task UpdateAsync(string id, Purchase updatedPurchase) =>
            await _purchasesCollection.ReplaceOneAsync(x => x.Id == id, updatedPurchase);

        public async Task RemoveAsync(string id) =>
            await _purchasesCollection.DeleteOneAsync(x => x.Id == id);
    }
}
