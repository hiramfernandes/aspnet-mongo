using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using Purchases.Application.Contracts;
using Purchases.Application.Models;
using Purchases.Application.Models.Settings;
using System.ClientModel;
using System.Text;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Purchases.Application.Services
{
    public class ReceiptRetrieverService : IReceiptRetrieverService
    {
        private readonly IPurchaseService _purchaseService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly OpenAiSettings _openAiSettings;
        private readonly TelegramBotClient _telegramBotClient;

        public ReceiptRetrieverService(
            IPurchaseService purchaseService,
            IHttpClientFactory httpClientFactory,
            IOptions<TelegramIntegrationSettings> telegramOptions,
            IOptions<OpenAiSettings> openAiOptions)
        {
            var telegramSettings = telegramOptions.Value;

            _purchaseService = purchaseService;
            _httpClientFactory = httpClientFactory;
            _openAiSettings = openAiOptions.Value;
            _telegramBotClient = new TelegramBotClient(telegramSettings.BotToken);
        }

        public async Task ProcessTelegramMessage(Update update, CancellationToken cancellationToken)
        {
            if (update.Message is not { } message)
                throw new InvalidOperationException($"Error when processing telegram update (null object)");

            //return BadRequest($"Error when processing telegram update (null object)");

            if (_openAiSettings.TestMode)
            {
                var jsonMessage = JsonSerializer.Serialize(message);
                await _telegramBotClient.SendMessage(message!.Chat.Id, $"Telegram message: {jsonMessage} ");
            }

            try
            {

                // TODO: Create factory to determine whether consume url or image/QR

                // Url Based info
                if (message.Text != null)
                {
                    var url = message.Text;
                    await HandleReceiptUrl(url, message!.Chat.Id, cancellationToken);
                } 
                else if (message?.Photo != null)
                {
                    await _telegramBotClient.SendMessage(message!.Chat.Id, "Image received! Processing...");

                    var fileId = message.Photo?.Last().FileId ?? message.Document?.FileId ?? throw new Exception("File not found");
                    await HandleQrCode(fileId, message!.Chat.Id, cancellationToken);

                    // await HandleImage(fileId, message!.Chat.Id, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                await _telegramBotClient.SendMessage(message!.Chat.Id, $"An error occurred: {ex.Message}");
            }
        }

        public async Task HandleReceiptUrl(string url, long messageId, CancellationToken cancellationToken)
        {
            var httpClient = _httpClientFactory.CreateClient("Scraper");
            using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseContentRead, cancellationToken);

            response.EnsureSuccessStatusCode();

            // Return the full HTML string
            var htmlContent = await response.Content.ReadAsStringAsync(cancellationToken);

            var systemPrompt = File.ReadAllText("Prompts/ExtractReceiptBasedOnHtmlContent.txt");

            var userMessage = $"""  
                        HTML: {htmlContent}  
                        URL: {url}  
                        """;

            var llmResponse = await SendInfoToLlmAsync(systemPrompt, userMessage);

            if (_openAiSettings.TestMode)
            {
                using var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(llmResponse));
                await _telegramBotClient.SendDocument(
                    messageId,
                    InputFile.FromStream(jsonStream, "receipt_parsed.json"));
            }

            var nfcReceipt = JsonSerializer.Deserialize<NfcReceipt>(llmResponse);

            await SavePurchaseAsync(nfcReceipt!, messageId, url);
        }

        public async Task HandleQrCode(string fileId, long chatMessageId, CancellationToken cancellationToken)
        {
            await _telegramBotClient.SendMessage(chatMessageId, "Image received! Processing...");
            var imageBinary = await GetImageBinaryData(fileId, cancellationToken);

            // Sending info to LLM - QR Code reader
            var qrCodeReaderPromt = await File.ReadAllTextAsync("Prompts/ExtractQrCodeBasedOnImage.txt");
            var qrDecodingOutput = await SendInfoToLlmAsync(qrCodeReaderPromt, imageBinary);

            await _telegramBotClient.SendMessage(
                chatMessageId,
                $"QR Image Analyzer obtained the following info: {qrDecodingOutput}"
            );
        }

        public async Task HandleImage(string fileId, long chatMessageId, CancellationToken cancellationToken)
        {
            // Sending info to LLM - Receipt
            var imageBinary = await GetImageBinaryData(fileId, cancellationToken);

            var promptMessage = File.ReadAllText("Prompts/ExtractReceiptBasedOnImage.txt");
            var modelAnalysisOutput = await SendInfoToLlmAsync(promptMessage, imageBinary);

            if (_openAiSettings.TestMode)
            {
                using var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(modelAnalysisOutput));
                await _telegramBotClient.SendDocument(
                    chatMessageId,
                    InputFile.FromStream(jsonStream, "receipt_parsed.json"));
            }

            var jsonOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
            };

            var obtainedReceiptData = JsonSerializer.Deserialize<NfcReceipt>(modelAnalysisOutput, jsonOptions);

            await SavePurchaseAsync(obtainedReceiptData!, chatMessageId);
        }

        private async Task<string> SendInfoToLlmAsync(
            string promptMessage,
            BinaryData imageBinary)
        {
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

        private async Task<BinaryData> GetImageBinaryData(string fileId, CancellationToken cancellationToken)
        {
            var fileInfo = await _telegramBotClient.GetFile(fileId!);

            using var stream = new MemoryStream();
            await _telegramBotClient.DownloadFile(fileInfo.FilePath!, stream, cancellationToken);

            stream.Position = 0;

            return BinaryData.FromStream(stream);
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
                ).ToArray(),
                UpdatedAtUtc = DateTime.UtcNow,
            };

            await _purchaseService.CreateAsync(purchase);
            await _telegramBotClient.SendMessage(chatId, $"Successfully persisted NF on purchases");
        }
    }
}
