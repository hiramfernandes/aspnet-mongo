using Telegram.Bot.Types;

namespace Purchases.Application.Contracts
{
    public interface IReceiptRetrieverService
    {
        Task ProcessTelegramMessage(Update update, CancellationToken cancellationToken);
        Task HandleImage(string fileId, long chatMessageId, CancellationToken cancellationToken);
        Task HandleQrCode(string fileId, long chatMessageId, CancellationToken cancellationToken);
        Task HandleReceiptUrl(string url, long messageId, CancellationToken cancellationToken);
    }
}