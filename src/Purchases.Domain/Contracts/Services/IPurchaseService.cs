using Purchases.Domain.Models;
using Purchases.Domain.Models.DTO.Purchase;

namespace Purchases.Domain.Contracts.Services;

public interface IPurchaseService
{
    Task CreateAsync(PurchaseDto newPurchase, CancellationToken cancellationToken);
    Task<IEnumerable<GetPurchaseDto>> GetAllAsync(int pageSize, CancellationToken cancellationToken);
    Task<Purchase?> GetAsync(string id, CancellationToken cancellationToken);
    Task RemoveAsync(string id);
    Task UpdateAsync(string id, PurchaseDto purchaseDto, CancellationToken cancellationToken);
}
