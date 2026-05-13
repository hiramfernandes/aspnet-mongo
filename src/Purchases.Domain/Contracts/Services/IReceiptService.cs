using Purchases.Domain.Models;

namespace Purchases.Domain.Contracts.Services;

public interface IReceiptService
{
    Task CreteAsync(Receipt receipt, CancellationToken cancellationToken);
}
