using aspnet_mongo.Models;
using aspnet_mongo.Models.Settings;
using aspnet_mongo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
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
        private readonly IPurchaseService _purchaseService;

        public CallbackController(
            IPurchaseService purchaseService,
            IOptions<TelegramIntegrationSettings> telegramOptions,
            IOptions<OpenAiSettings> openAiOptions)
        {
            var telegramSettings = telegramOptions.Value;

            _openAiSettings = openAiOptions.Value;
            _telegramBotClient = new TelegramBotClient(telegramSettings.BotToken);
            _purchaseService = purchaseService;
        }

        [HttpPost("telegram")]
        public async Task<IActionResult> TelegramWebhook([FromBody] Update update, CancellationToken cancellationToken)
        {
            if (update.Message is not { } message)
                return BadRequest($"Error when processing telegram update (null object)");

            // Callback - used to obtain a Telegram payload for further testing (locally)

            if (_openAiSettings.TestMode)
            {
                var jsonMessage = System.Text.Json.JsonSerializer.Serialize(message);
                await _telegramBotClient.SendMessage(message!.Chat.Id, $"Telegram message: {jsonMessage} ");

                return Ok();
            }

            if (message?.Photo != null ||
                message?.Document != null)
            {
                await _telegramBotClient.SendMessage(message!.Chat.Id, "🧾 Receipt received! Processing...");

                try
                {
                    // Getting file info and downloading it
                    var fileId = message.Photo?.Last().FileId ?? message.Document?.FileId;
                    var fileInfo = await _telegramBotClient.GetFile(fileId!);

                    using var stream = new MemoryStream();
                    await _telegramBotClient.DownloadFile(fileInfo.FilePath!, stream, cancellationToken);

                    stream.Position = 0;

                    var imageBinary = BinaryData.FromStream(stream);

                    // Sending info to LLM
                    var promptMessage = System.IO.File.ReadAllText("Prompts/ImprovedWithNumbers.txt");
                    var aiChatMessage = new UserChatMessage(
                        ChatMessageContentPart.CreateTextPart(promptMessage),
                        ChatMessageContentPart.CreateImagePart(imageBinary, "image/jpeg")
                    );

                    var client = GetChatClient();

                    var completion = await client.CompleteChatAsync(
                        [aiChatMessage]
                    );

                    // If this is the full prompt, this returns a json containing all the recepit info
                    var modelAnalysisOutput = completion.Value.Content[0].Text;

                    //var modelAnalysisOutput = System.IO.File.ReadAllText("Receipts/receipt1.json");

                    var jsonOptions = new JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = true,
                    };

                    var obtainedReceiptData = JsonSerializer.Deserialize<NfcReceipt>(modelAnalysisOutput, jsonOptions);

                    if (obtainedReceiptData == null)
                        await _telegramBotClient.SendMessage(message!.Chat.Id, "Unable to parse received message");

                    var vendorName = obtainedReceiptData!.Merchant?.LegalName ?? obtainedReceiptData.Merchant?.TradeName;
                    //var vendor = _vendorService.GetByName(vendorName);

                    //if (vendor == null)
                    //{
                    //    var newVendor = new CreateVendorDto()
                    //    {
                    //        Location = obtainedReceiptData.Merchant?.Address?.City,
                    //        Name = vendorName,
                    //        LogoUrl = null
                    //    };

                    //    await _vendorService.CreateVendor(newVendor, cancellationToken);
                    //}

                    var purchase = new Purchase()
                    {
                        PurchaseDate = DateTime.TryParse(obtainedReceiptData?.Transaction?.IssueDatetime, out var purchaseDate) ? purchaseDate : null,
                        PurchaseUrl = obtainedReceiptData?.QR?.Url,
                        VendorName = vendorName,
                        VendorId = null,
                        TotalAmount = obtainedReceiptData!.Totals?.Total,
                        Items = obtainedReceiptData!.Items?.Select(x => x.DescriptionRaw ?? string.Empty).ToArray()
                    };

                    await _purchaseService.CreateAsync(purchase);
                    await _telegramBotClient.SendMessage(message!.Chat.Id, $"Successfully persisted NF on purchases");
                }
                catch (Exception ex)
                {
                    await _telegramBotClient.SendMessage(message!.Chat.Id, $"An error occurred: {ex.Message}");
                }
            }

            return Ok();
        }

        private ChatClient GetChatClient()
        {
            var apiKey = _openAiSettings.ApiKey;
            var model = _openAiSettings.Model;
            var endpoint = new Uri(_openAiSettings.Endpoint!);

            var credential = new ApiKeyCredential(apiKey!);

            var clientOptions = new OpenAIClientOptions
            {
                Endpoint = endpoint
            };

            var openAIClient = new OpenAIClient(credential, clientOptions);
            var client = openAIClient.GetChatClient(model);

            return client;
        }
    }
}
