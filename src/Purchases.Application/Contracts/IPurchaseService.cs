using Purchases.Application.Models;
using Purchases.Application.Models.DTO.Purchase;

namespace Purchases.Application.Contracts
{
    public interface IPurchaseService
    {
        Task CreateAsync(Purchase newPurchase);
        Task<IEnumerable<GetPurchaseDto>> GetAllAsync(int pageSize, CancellationToken cancellationToken);
        Task<Purchase?> GetAsync(string id, CancellationToken cancellationToken);
        Task RemoveAsync(string id);
        Task UpdateAsync(string id, Purchase updatedPurchase);
    }
}
