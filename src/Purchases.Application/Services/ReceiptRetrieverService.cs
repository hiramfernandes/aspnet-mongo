using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using Purchases.Application.Contracts;
using Purchases.Application.Models;
using Purchases.Application.Models.Settings;
using Purchases.Application.Properties;
using System.ClientModel;
using System.Text;
using System.Text.Json;
using Telegram.Bot.Types;

namespace Purchases.Application.Services
{
    public class ReceiptRetrieverService : IReceiptRetrieverService
    {
        private readonly IPurchaseService _purchaseService;
        private readonly IMessageNotifier _messageNotifier;
        private readonly IRemoteFileManager _remoteFileManager;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly OpenAiSettings _openAiSettings;

        public ReceiptRetrieverService(
            IPurchaseService purchaseService,
            IMessageNotifier messageNotifier,
            IRemoteFileManager remoteFileManager,
            IHttpClientFactory httpClientFactory,
            IOptions<OpenAiSettings> openAiOptions)
        {
            _purchaseService = purchaseService;
            _messageNotifier = messageNotifier;
            _remoteFileManager = remoteFileManager;
            _httpClientFactory = httpClientFactory;
            _openAiSettings = openAiOptions.Value;
        }
        
        #region Receipt Handling
        public async Task HandleReceiptUrl(string url, long messageId, CancellationToken cancellationToken)
        {
            var httpClient = _httpClientFactory.CreateClient("Scraper");
            using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseContentRead, cancellationToken);

            response.EnsureSuccessStatusCode();

            if (_openAiSettings.TestMode)
            {
                await _messageNotifier.SendMessage(messageId, "URL content retrieved. Processing...");
            }

            // Return the full HTML string
            var htmlContent = await response.Content.ReadAsStringAsync(cancellationToken);

            var systemPrompt = Resources.ExtractReceiptBasedOnUrlInfo;

            var userMessage = $"""  
                        HTML: {htmlContent}  
                        URL: {url}  
                        """;

            var llmResponse = await SendInfoToLlmAsync(systemPrompt, userMessage);

            if (_openAiSettings.TestMode)
            {
                using var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(llmResponse));
                await _messageNotifier.SendDocument(
                    messageId,
                    InputFile.FromStream(jsonStream, "receipt_parsed.json"));
            }

            var nfcReceipt = JsonSerializer.Deserialize<NfcReceipt>(llmResponse);

            await SavePurchaseAsync(nfcReceipt!, messageId, url);
        }

        public async Task HandleQrCode(string fileId, long chatMessageId, CancellationToken cancellationToken)
        {
            await _messageNotifier.SendMessage(chatMessageId, "Image received! Processing...");
            var imageBinary = await GetImageBinaryData(fileId, cancellationToken);

            // Sending info to LLM - QR Code reader
            var qrCodeReaderPromt = Resources.ExtractQrCodeBasedOnImage;
            var qrDecodingOutput = await SendInfoToLlmAsync(qrCodeReaderPromt, imageBinary);

            await _messageNotifier.SendMessage(
                chatMessageId,
                $"QR Image Analyzer obtained the following info: {qrDecodingOutput}"
            );
        }

        public async Task HandleImage(string fileId, long chatMessageId, CancellationToken cancellationToken)
        {
            // Sending info to LLM - Receipt
            var imageBinary = await GetImageBinaryData(fileId, cancellationToken);

            var promptMessage = Resources.ExtractReceiptBasedOnImage;
            var modelAnalysisOutput = await SendInfoToLlmAsync(promptMessage, imageBinary);

            if (_openAiSettings.TestMode)
            {
                using var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(modelAnalysisOutput));
                await _messageNotifier.SendDocument(
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
        #endregion Receipt Handling

        #region LLM Processing

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
        #endregion LLM Processing

        private async Task<BinaryData> GetImageBinaryData(string fileId, CancellationToken cancellationToken)
        {
            var fileInfo = await _remoteFileManager.GetFile(fileId!);

            using var stream = new MemoryStream();
            await _remoteFileManager.DownloadFile(fileInfo.FilePath!, stream, cancellationToken);

            stream.Position = 0;

            return BinaryData.FromStream(stream);
        }

        private async Task SavePurchaseAsync(NfcReceipt obtainedReceiptData, long chatId, string? url = null)
        {
            var vendorName = obtainedReceiptData!.Merchant?.LegalName ?? obtainedReceiptData.Merchant?.TradeName;

            if (!DateTime.TryParse(obtainedReceiptData?.Transaction?.IssueDatetime, out var purchaseDate))
            {
                await _messageNotifier.SendMessage(chatId, $"Error parsing purchase date");
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
            await _messageNotifier.SendMessage(chatId, $"Successfully persisted NF on purchases");
        }
    }
}
