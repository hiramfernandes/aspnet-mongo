using Microsoft.AspNetCore.Mvc;
using Purchases.Application.Contracts;
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
                await TelegramServiceRouter(update.Message, null, cancellationToken);

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
                await _receiptRetrieverService.HandleReceiptUrl(url, message?.Chat.Id ?? default, cancellationToken);
            }
            else if (message?.Photo != null)
            {
                var fileId = message.Photo?.Last().FileId ?? message.Document?.FileId ?? throw new Exception("File not found");
                await _receiptRetrieverService.HandleImage(fileId, message?.Chat.Id ?? default, cancellationToken);
            }
        }
    }
}
