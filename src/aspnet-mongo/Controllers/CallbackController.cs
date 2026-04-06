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
                await _receiptRetrieverService.ProcessTelegramMessage(update, cancellationToken);
                return Ok();
            }
            catch (Exception exc)
            {
                return BadRequest(exc.Message);
            }
        }
    }
}
