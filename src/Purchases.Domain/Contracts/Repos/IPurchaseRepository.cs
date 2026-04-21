using Purchases.Domain.Models;

namespace Purchases.Domain.Contracts.Repos
{
    public interface IPurchaseRepository
    {
        Task<IEnumerable<Purchase>> GetAllAsync(int pageSize, CancellationToken cancellationToken);
        Task<Purchase?> GetAsync(string id, CancellationToken cancellationToken);
        Task<Purchase> GetByUrlAsync(string url, CancellationToken cancellationToken);
        Task CreateAsync(Purchase newPurchase);
        Task RemoveAsync(string id);
        Task UpdateAsync(string id, Purchase updatedPurchase);
    }
}
