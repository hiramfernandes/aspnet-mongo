using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace aspnet_mongo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CallbackController : ControllerBase
    {
        private readonly TelegramBotClient _telegramBotClient;
        private readonly ILogger<CallbackController> _logger;

        public CallbackController(
            //ITelegramBotClient telegramBotClient,
            ILogger<CallbackController> logger)
        {
            _telegramBotClient = new TelegramBotClient("8228945569:AAEkD24cO01thWhv6qJxCbyACbeDdYDfEa0");
            _logger = logger;
        }

        [HttpPost("telegram")]
        public async Task<IActionResult> TelegramWebhook([FromBody] Update update)
        {
            if (update.Message is not { } message)
                return BadRequest($"Error when processing telegram update (null object)");

            if (message?.Photo != null ||
                message?.Document != null)
            {
                await _telegramBotClient.SendMessage(message!.Chat.Id, "🧾 Receipt received! Processing...");

                //var fileId = message.Photo?.Last().FileId ?? message.Document?.FileId;
                //var fileInfo = await _telegramBotClient.GetFile(fileId!);
                //using var stream = new MemoryStream();
                //await _telegramBotClient.DownloadFile(fileInfo.FilePath!, stream);

                //System.IO.File.WriteAllBytes(@"C:\Temp\test.jpg", stream.ToArray());
            }

            return Ok();
        }
    }
}
