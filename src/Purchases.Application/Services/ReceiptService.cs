using Purchases.Domain.Contracts.Repos;
using Purchases.Domain.Contracts.Services;
using Purchases.Domain.Models;

namespace Purchases.Application.Services;

public class ReceiptService : IReceiptService
{
    private readonly IReceiptRepository _repository;

    public ReceiptService(IReceiptRepository repository)
    {
        _repository = repository;
    }
    
    public async Task CreteAsync(Receipt receipt, CancellationToken cancellationToken)
    {
        await _repository.CreateAsync(receipt, cancellationToken);
    }
}