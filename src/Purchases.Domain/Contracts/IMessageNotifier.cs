using Telegram.Bot.Types;

namespace Purchases.Domain.Contracts;

public interface IMessageNotifier
{
    public Task SendMessage(long messageId, string message);
    public Task SendDocument(long messageId, InputFile inputFile);
}
