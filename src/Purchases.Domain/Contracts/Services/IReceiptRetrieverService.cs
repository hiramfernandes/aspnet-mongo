namespace Purchases.Domain.Contracts.Services;

public interface IReceiptRetrieverService
{
    Task HandleImage(string fileId, long chatMessageId, CancellationToken cancellationToken);
    Task HandleQrCode(string fileId, long chatMessageId, CancellationToken cancellationToken);
    Task HandleReceiptUrl(string url, long messageId, CancellationToken cancellationToken);
}