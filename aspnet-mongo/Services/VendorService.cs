﻿using aspnet_mongo.Models;
using aspnet_mongo.Models.DTO;
using aspnet_mongo.Models.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace aspnet_mongo.Services
{
    public interface IVendorService
    {
        Task<IEnumerable<Vendor>> GetAllAsync();
        Task<Vendor> GetById(string id);
        Task CreateVendor(CreateVendorDto vendorDto, CancellationToken cancellationToken);
    }

    public class VendorService : IVendorService
    {

        private readonly string _collectionName = "vendors";
        private readonly IMongoCollection<Vendor> _vendorsCollection;

        public VendorService(IOptions<MongoDbSettings> databaseSettings)
        {
            var connectionString = databaseSettings.Value.ConnectionString;
            var dbName = databaseSettings.Value.DatabaseName;

            var mongoClient = new MongoClient(connectionString);
            var mongoDatabase = mongoClient.GetDatabase(dbName);

            _vendorsCollection = mongoDatabase.GetCollection<Vendor>(_collectionName);
        }

        public async Task<IEnumerable<Vendor>> GetAllAsync() =>
            await _vendorsCollection.Find(_ => true).ToListAsync();

        public async Task<Vendor> GetById(string id)
        {
            var vendor = await _vendorsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
            return vendor;
        }

        public async Task CreateVendor(CreateVendorDto vendorDto, CancellationToken cancellationToken)
        {
            var vendor = new Vendor()
            { 
                Name = vendorDto.Name,
                Location = vendorDto.Location,
                LogoUrl = vendorDto.LogoUrl,
                AddedOn = DateTime.Now,
                UpdatedOn = DateTime.Now
            };

            InsertOneOptions options = new InsertOneOptions
            {
                BypassDocumentValidation = false,
                Comment = "Added using asp.net backend"
            };

            await _vendorsCollection.InsertOneAsync(vendor, options, cancellationToken);
        }
    }
}
