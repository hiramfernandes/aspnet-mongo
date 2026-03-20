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

        private readonly IHttpClientFactory _httpClientFactory;

        public CallbackController(
            IPurchaseService purchaseService,
            IOptions<TelegramIntegrationSettings> telegramOptions,
            IOptions<OpenAiSettings> openAiOptions,
            IHttpClientFactory httpClientFactory)
        {
            var telegramSettings = telegramOptions.Value;

            _openAiSettings = openAiOptions.Value;
            _telegramBotClient = new TelegramBotClient(telegramSettings.BotToken);
            _purchaseService = purchaseService;
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost("telegram")]
        public async Task<IActionResult> TelegramWebhook([FromBody] Update update, CancellationToken cancellationToken)
        {
            if (update.Message is not { } message)
                return BadRequest($"Error when processing telegram update (null object)");

            // Callback - used to obtain a Telegram payload for further testing (locally)

            if (_openAiSettings.TestMode)
            {
                var jsonMessage = JsonSerializer.Serialize(message);
                await _telegramBotClient.SendMessage(message!.Chat.Id, $"Telegram message: {jsonMessage} ");
            }

            // Url Based info
            if (message.Text != null)
            {
                var url = message.Text;
                var httpClient = _httpClientFactory.CreateClient("Scraper");
                try
                {
                    using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseContentRead, cancellationToken);

                    response.EnsureSuccessStatusCode();

                    // Return the full HTML string
                    var htmlContent = await response.Content.ReadAsStringAsync(cancellationToken);

                    var systemPrompt = System.IO.File.ReadAllText("Prompts/ExtractReceiptBasedOnHtmlContent.txt");

                    var userMessage = $"""  
                        HTML: {htmlContent}  
                        URL: {url}  
                        """;

                    var llmResponse = await SendInfoToLlmAsync(systemPrompt, userMessage);

                    if (_openAiSettings.TestMode)
                    {
                        using var jsonStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(llmResponse));
                        await _telegramBotClient.SendDocument(
                            message!.Chat.Id,
                            InputFile.FromStream(jsonStream, "receipt_parsed.json"));
                    }

                    var nfcReceipt = JsonSerializer.Deserialize<NfcReceipt>(llmResponse);

                    await SavePurchaseAsync(nfcReceipt!, message.Chat.Id, url);
                }
                catch (HttpRequestException ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            if (message?.Photo != null)
            {
                await _telegramBotClient.SendMessage(message!.Chat.Id, "Image received! Processing...");

                try
                {
                    // Getting file info and downloading it
                    var fileId = message.Photo?.Last().FileId ?? message.Document?.FileId;
                    var fileInfo = await _telegramBotClient.GetFile(fileId!);

                    using var stream = new MemoryStream();
                    await _telegramBotClient.DownloadFile(fileInfo.FilePath!, stream, cancellationToken);

                    stream.Position = 0;

                    var imageBinary = BinaryData.FromStream(stream);

                    // Sending info to LLM - QR Code reader
                    var qrCodeReaderPromt = await System.IO.File.ReadAllTextAsync("Prompts/ExtractQrCodeBasedOnImage.txt");
                    var qrDecodingOutput = await SendInfoToLlmAsync(qrCodeReaderPromt, imageBinary);

                    await _telegramBotClient.SendMessage(
                        message!.Chat.Id,
                        $"QR Image Analyzer obtained the following info: {qrDecodingOutput}"
                    );

                    return Ok();

                    // Sending info to LLM - Receipt
                    var promptMessage = System.IO.File.ReadAllText("Prompts/ExtractReceiptBasedOnImage.txt");
                    var modelAnalysisOutput = await SendInfoToLlmAsync(promptMessage, imageBinary);

                    if (_openAiSettings.TestMode)
                    {
                        using var jsonStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(modelAnalysisOutput));
                        await _telegramBotClient.SendDocument(
                            message!.Chat.Id,
                            InputFile.FromStream(jsonStream, "receipt_parsed.json"));
                    }

                    var jsonOptions = new JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = true,
                    };

                    var obtainedReceiptData = JsonSerializer.Deserialize<NfcReceipt>(modelAnalysisOutput, jsonOptions);

                    await SavePurchaseAsync(obtainedReceiptData!, message.Chat.Id);
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

        private async Task<string> SendInfoToLlmAsync(
            string promptMessage,
            BinaryData imageBinary)
        {
            //var promptMessage = System.IO.File.ReadAllText("Prompts/ExtractReceiptBasedOnImage.txt");
            var aiChatMessage = new UserChatMessage(
                ChatMessageContentPart.CreateTextPart(promptMessage),
                ChatMessageContentPart.CreateImagePart(imageBinary, "image/jpeg")
            );

            var client = GetChatClient();

            var completion = await client.CompleteChatAsync(
                [aiChatMessage]
            );

            var modelAnalysisOutput = completion.Value.Content[0].Text;

            return modelAnalysisOutput;
        }

        private async Task<string> SendInfoToLlmAsync(
            string systemPrompt,
            string userMessage)
        {
            var chatMessages = new List<ChatMessage>()
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userMessage)
            };

            var client = GetChatClient();

            var response = await client.CompleteChatAsync(
                chatMessages,
                new ChatCompletionOptions()
                {
                    Temperature = 0
                }
            );

            var modelAnalysisOutput = response.Value.Content[0].Text;

            return modelAnalysisOutput;
        }

        private async Task SavePurchaseAsync(NfcReceipt obtainedReceiptData, ChatId chatId, string? url = null)
        {
            var vendorName = obtainedReceiptData!.Merchant?.LegalName ?? obtainedReceiptData.Merchant?.TradeName;

            if (!DateTime.TryParse(obtainedReceiptData?.Transaction?.IssueDatetime, out var purchaseDate))
            {
                await _telegramBotClient.SendMessage(chatId, $"Error parsing purchase date");
                return;
            }

            var purchase = new Purchase()
            {
                PurchaseDate = purchaseDate.Date,
                PurchaseUrl = url ?? obtainedReceiptData?.QR?.Url,
                VendorName = vendorName,
                VendorId = null,
                TotalAmount = obtainedReceiptData!.Totals?.Total,
                Items = obtainedReceiptData!.Items?.Select(item =>
                    new PurchaseItem()
                    {
                        Description = item.DescriptionRaw,
                        Tags = item.Tags?.ToArray(),
                        UnitPrice = (float?)item.UnitPrice
                    }
                ).ToArray()
            };

            await _purchaseService.CreateAsync(purchase);
            await _telegramBotClient.SendMessage(chatId, $"Successfully persisted NF on purchases");
        }
    }
}
