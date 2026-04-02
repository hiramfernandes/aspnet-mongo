using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Purchases.Application.Contracts;
using Purchases.Application.Models.Settings;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace aspnet_mongo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CallbackController : ControllerBase
    {
        private readonly TelegramBotClient _telegramBotClient;
        private readonly OpenAiSettings _openAiSettings;
        private readonly IReceiptRetrieverService _receiptRetrieverService;

        public CallbackController(
            IReceiptRetrieverService receiptRetrieverService,
            IOptions<TelegramIntegrationSettings> telegramOptions,
            IOptions<OpenAiSettings> openAiOptions)
        {
            var telegramSettings = telegramOptions.Value;

            _openAiSettings = openAiOptions.Value;
            _telegramBotClient = new TelegramBotClient(telegramSettings.BotToken);
            _receiptRetrieverService = receiptRetrieverService;
        }

        [HttpPost("telegram")]
        public async Task<IActionResult> TelegramWebhook([FromBody] Update update, CancellationToken cancellationToken)
        {
            if (update.Message is not { } message)
                return BadRequest($"Error when processing telegram update (null object)");

            if (_openAiSettings.TestMode)
            {
                var jsonMessage = JsonSerializer.Serialize(message);
                await _telegramBotClient.SendMessage(message!.Chat.Id, $"Telegram message: {jsonMessage} ");
            }

            try
            {
                // Url Based info
                if (message.Text != null)
                {
                    var url = message.Text;
                    await _receiptRetrieverService.HandleReceiptUrl(url, message!.Chat.Id, cancellationToken);
                }

                if (message?.Photo != null)
                {
                    await _telegramBotClient.SendMessage(message!.Chat.Id, "Image received! Processing...");

                    // Getting file info and downloading it
                    var fileId = message.Photo?.Last().FileId ?? message.Document?.FileId ?? throw new Exception("File not found");
                    await _receiptRetrieverService.HandleQrCode(fileId, message!.Chat.Id, cancellationToken);

                    // await HandleImage(fileId, message!.Chat.Id, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                await _telegramBotClient.SendMessage(message!.Chat.Id, $"An error occurred: {ex.Message}");
            }

            return Ok();
        }
    }
}
