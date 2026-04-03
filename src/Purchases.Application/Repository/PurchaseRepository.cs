using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Purchases.Application.Models;
using Purchases.Application.Models.Settings;

namespace Purchases.Application.Repository
{
    public interface IPurchaseRepository
    {
        Task<IEnumerable<Purchase>> GetAllAsync(int pageSize, CancellationToken cancellationToken);
        Task<Purchase?> GetAsync(string id, CancellationToken cancellationToken);
        Task CreateAsync(Purchase newPurchase);
        Task RemoveAsync(string id);
        Task UpdateAsync(string id, Purchase updatedPurchase);
    }

    public class PurchaseRepository : IPurchaseRepository
    {
        private readonly IMongoCollection<Purchase> _purchasesCollection;

        private readonly string _collectionName = "purchases";

        public PurchaseRepository(IOptions<MongoDbSettings> databaseSettings)
        {
            var connectionString = databaseSettings.Value.ConnectionString;
            var dbName = databaseSettings.Value.DatabaseName;

            var mongoClient = new MongoClient(connectionString);
            var mongoDatabase = mongoClient.GetDatabase(dbName);

            _purchasesCollection = mongoDatabase.GetCollection<Purchase>(_collectionName);
        }

        public async Task<IEnumerable<Purchase>> GetAllAsync(int pageSize, CancellationToken cancellationToken)
        {
            var queryableCollection = _purchasesCollection.AsQueryable();
            return await queryableCollection
                .OrderByDescending(x => x.PurchaseDate)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<Purchase?> GetAsync(string id, CancellationToken cancellationToken) =>
            await _purchasesCollection.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);

        public async Task CreateAsync(Purchase newPurchase) =>
            await _purchasesCollection.InsertOneAsync(newPurchase);

        public async Task UpdateAsync(string id, Purchase updatedPurchase) =>
            await _purchasesCollection.ReplaceOneAsync(x => x.Id == id, updatedPurchase);

        public async Task RemoveAsync(string id) =>
            await _purchasesCollection.DeleteOneAsync(x => x.Id == id);
    }
}
