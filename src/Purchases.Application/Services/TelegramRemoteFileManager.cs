using Microsoft.Extensions.Options;
using Purchases.Application.Contracts;
using Purchases.Application.Models.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Purchases.Application.Services
{
    public class TelegramRemoteFileManager : IRemoteFileManager
    {
        private readonly TelegramBotClient _telegramBotClient;

        public TelegramRemoteFileManager(
            IOptions<TelegramIntegrationSettings> telegramOptions)
        {
            var telegramSettings = telegramOptions.Value;

            _telegramBotClient = new TelegramBotClient(telegramSettings.BotToken);
        }

        public async Task DownloadFile(string filePath, Stream stream, CancellationToken cancellationToken)
        {
            await _telegramBotClient.DownloadFile(filePath, stream, cancellationToken);
        }

        public async Task<TGFile> GetFile(string fileId)
        {
            return await _telegramBotClient.GetFile(fileId);
        }
    }
}
