using Purchases.Domain.Models;

namespace Purchases.Domain.Contracts.Repos
{
    public interface IVendorRepository
    {
        Task CreateAsync(Vendor vendor, CancellationToken cancellationToken);
        Task<IEnumerable<Vendor>> GetAllAsync();
        Task<Vendor> GetAsync(string id);
        Task<Vendor?> GetByNameAsync(string name, CancellationToken cancellationToken);
        Task RemoveAsync(string id, CancellationToken cancellationToken);
        Task UpdateVendor(string id, Vendor vendor, CancellationToken cancellationToken);
    }
}
