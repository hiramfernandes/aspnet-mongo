using Microsoft.AspNetCore.Mvc;
using Purchases.Domain.Contracts.Services;
using Telegram.Bot.Types;

namespace aspnet_mongo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CallbackController : ControllerBase
    {
        private readonly IReceiptRetrieverService _receiptRetrieverService;

        public CallbackController(IReceiptRetrieverService receiptRetrieverService)
        {
            _receiptRetrieverService = receiptRetrieverService;
        }

        [HttpPost("telegram")]
        public async Task<IActionResult> TelegramWebhook(
            [FromBody] Update update, 
            CancellationToken cancellationToken)
        {
            try
            {
                //await TelegramServiceRouter(update.Message, null, cancellationToken);

                return Ok();
            }
            catch (Exception exc)
            {
                return BadRequest(exc.Message);
            }
        }

        [HttpPost("url")]
        public async Task<IActionResult> ProcessUrl(
            [FromBody] string url,
            CancellationToken cancellationToken)
        {
            if (!ValidUrl(url))
                return BadRequest("Invalid URL");

            await _receiptRetrieverService.HandleReceiptUrl(url, default, cancellationToken);

            return Ok();
        }

        private async Task TelegramServiceRouter(
           Message? message,
           string? providedUrl,
           CancellationToken cancellationToken)
        {
            if (message?.Text != null)
            {
                var url = message.Text;
                if (!ValidUrl(url))
                    throw new InvalidOperationException("Invalid URL");

                await _receiptRetrieverService.HandleReceiptUrl(url, message?.Chat.Id ?? default, cancellationToken);
            }
            else if (message?.Photo != null)
            {
                var fileId = message.Photo?.LastOrDefault()?.FileId ?? message.Document?.FileId ?? throw new Exception("File not found");
                await _receiptRetrieverService.HandleImage(fileId, message?.Chat.Id ?? default, cancellationToken);
            }
        }

        private bool ValidUrl(string url)
        {
            // Validate URL
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return false;

            if ((uri.Scheme != "http" && uri.Scheme != "https") ||
                 uri.Host.Contains("localhost") ||
                 uri.Host.StartsWith("192.168.") ||
                 uri.Host.StartsWith("10.") ||
                 uri.Host == "127.0.0.1")
                return false;

            return true;
        }
    }
}
