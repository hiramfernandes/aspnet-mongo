using aspnet_mongo.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Purchases.Application.Models.Settings;

namespace Purchases.Application.Repository
{
    public interface IVendorRepository
    {
        Task CreateAsync(Vendor vendor, CancellationToken cancellationToken);
        Task CreateVendor(Vendor vendor, CancellationToken cancellationToken);
        Task<IEnumerable<Vendor>> GetAllAsync();
        Task<Vendor> GetAsync(string id);
        Task<Vendor?> GetByNameAsync(string name, CancellationToken cancellationToken);
        Task RemoveAsync(string id, CancellationToken cancellationToken);
        Task UpdateVendor(string id, Vendor vendor, CancellationToken cancellationToken);
    }

    public class VendorRepository : IVendorRepository
    {
        private readonly string _collectionName = "vendors";

        private readonly IMongoCollection<Vendor> _vendorsCollection;

        public VendorRepository(
            IOptions<MongoDbSettings> databaseSettings)
        {
            var connectionString = databaseSettings.Value.ConnectionString;
            var dbName = databaseSettings.Value.DatabaseName;

            var mongoClient = new MongoClient(connectionString);
            var mongoDatabase = mongoClient.GetDatabase(dbName);

            _vendorsCollection = mongoDatabase.GetCollection<Vendor>(_collectionName);
        }

        public async Task<IEnumerable<Vendor>> GetAllAsync()
        {
            return await _vendorsCollection.Find(_ => true).ToListAsync();
        }

        public async Task<Vendor> GetAsync(string id)
        {
            return await _vendorsCollection
                .Find(x => x.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Vendor vendor, CancellationToken cancellationToken)
        {
            InsertOneOptions options = new InsertOneOptions
            {
                BypassDocumentValidation = false,
                Comment = "Added using asp.net backend"
            };

            await _vendorsCollection.InsertOneAsync(vendor, options, cancellationToken);
        }

        public async Task<Vendor?> GetByNameAsync(string name, CancellationToken cancellationToken)
        {
            return await _vendorsCollection
                .Find(x => x.Name == name)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task CreateVendor(Vendor vendor, CancellationToken cancellationToken)
        {
            InsertOneOptions options = new InsertOneOptions
            {
                BypassDocumentValidation = false,
                Comment = "Added using asp.net backend"
            };

            await _vendorsCollection.InsertOneAsync(vendor, options, cancellationToken);
        }

        public async Task UpdateVendor(string id, Vendor vendor, CancellationToken cancellationToken)
        {
            await _vendorsCollection.ReplaceOneAsync(x => x.Id == id, vendor, cancellationToken: cancellationToken);
        }

        public async Task RemoveAsync(string id, CancellationToken cancellationToken) =>
            await _vendorsCollection.DeleteOneAsync(x => x.Id == id, cancellationToken);
    }
}
