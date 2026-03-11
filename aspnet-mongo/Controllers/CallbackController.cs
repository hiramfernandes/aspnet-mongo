using aspnet_mongo.Models.Settings;
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

        public CallbackController(
            IOptions<TelegramIntegrationSettings> telegramOptions,
            IOptions<OpenAiSettings> openAiOptions)
        {
            var telegramSettings = telegramOptions.Value;

            _openAiSettings = openAiOptions.Value;
            _telegramBotClient = new TelegramBotClient(telegramSettings.BotToken);
        }

        [HttpPost("telegram")]
        public async Task<IActionResult> TelegramWebhook([FromBody] Update update, CancellationToken cancellationToken)
        {
            if (update.Message is not { } message)
                return BadRequest($"Error when processing telegram update (null object)");

            if (message?.Photo != null ||
                message?.Document != null)
            {
                await _telegramBotClient.SendMessage(message!.Chat.Id, "🧾 Receipt received! Processing...");

                var messageSerialized = JsonSerializer.Serialize(message);

                await _telegramBotClient.SendMessage(message!.Chat.Id, $"Here's the object received: {messageSerialized}");
                var fileId = message.Photo?.Last().FileId ?? message.Document?.FileId;
                var fileInfo = await _telegramBotClient.GetFile(fileId!);

                using var stream = new MemoryStream();
                await _telegramBotClient.DownloadFile(fileInfo.FilePath!, stream, cancellationToken);

                var imageBinary = BinaryData.FromStream(stream);
                var promptMessage = System.IO.File.ReadAllText("Prompts/WhatDoYouSee.txt");
                var aiChatMessage = new UserChatMessage(
                    ChatMessageContentPart.CreateTextPart(promptMessage),
                    ChatMessageContentPart.CreateImagePart(imageBinary, "image/jpeg")
                );

                var client = GetChatClient();

                var completion = await client.CompleteChatAsync(
                    [aiChatMessage]
                );
                var modelAnalysisOutput = completion.Value.Content[0].Text;

                await _telegramBotClient.SendMessage(message!.Chat.Id, $"Here's the output analysis: {modelAnalysisOutput}");
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
