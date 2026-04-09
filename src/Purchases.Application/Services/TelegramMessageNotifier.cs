using Microsoft.Extensions.Options;
using Purchases.Application.Contracts;
using Purchases.Application.Models.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Purchases.Application.Services
{
    public class TelegramMessageNotifier : IMessageNotifier
    {
        private readonly TelegramBotClient _telegramBotClient;

        public TelegramMessageNotifier(
            IOptions<TelegramIntegrationSettings> telegramOptions)
        {
            var telegramSettings = telegramOptions.Value;

            _telegramBotClient = new TelegramBotClient(telegramSettings.BotToken);
        }

        public async Task SendMessage(long messageId, string message)
        {
            if (message != default)
            {
                await _telegramBotClient.SendMessage(messageId, message);
            }
        }
        public async Task SendDocument(long messageId, InputFile inputFile)
        {
            await _telegramBotClient.SendDocument(messageId, inputFile);
        }
    }
}
