using Microsoft.Extensions.Options;
using Purchases.Application.Properties;
using Purchases.Domain.Contracts.Services;
using Purchases.Domain.Models;
using Purchases.Domain.Models.Settings;
using System.Text;
using System.Text.Json;
using Telegram.Bot.Types;

namespace Purchases.Application.Services
{
    public class ReceiptRetrieverService : IReceiptRetrieverService
    {
        private readonly IPurchaseService _purchaseService;
        private readonly ILlmProcessor _llmProcessor;
        private readonly IMessageNotifier _messageNotifier;
        private readonly IRemoteFileManager _remoteFileManager;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly OpenAiSettings _openAiSettings;

        public ReceiptRetrieverService(
            IPurchaseService purchaseService,
            ILlmProcessor llmProcessor,
            IMessageNotifier messageNotifier,
            IRemoteFileManager remoteFileManager,
            IHttpClientFactory httpClientFactory,
            IOptions<OpenAiSettings> openAiOptions)
        {
            _purchaseService = purchaseService;
            _llmProcessor = llmProcessor;
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

            var llmResponse = await _llmProcessor.Analyze(systemPrompt, userMessage);

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
            var qrDecodingOutput = await _llmProcessor.Analyze(qrCodeReaderPromt, imageBinary);

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
            var modelAnalysisOutput = await _llmProcessor.Analyze(promptMessage, imageBinary);

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
