using Purchases.Domain.Models;

namespace Purchases.Domain.Contracts.Repos;

public interface IReceiptRepository
{
    Task CreateAsync(Receipt newReceipt, CancellationToken cancellationToken);
}