using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using Purchases.Domain.Contracts;
using Purchases.Domain.Models.Settings;
using System.ClientModel;

namespace Purchases.Application.Services
{
    public class LlmProcessor : ILlmProcessor
    {
        private readonly OpenAiSettings _openAiSettings;

        public LlmProcessor(IOptions<OpenAiSettings> openAiOptions)
        {
            _openAiSettings = openAiOptions.Value;
        }

        public async Task<string> Analyze(
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

        public async Task<string> Analyze(
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

    }
}
