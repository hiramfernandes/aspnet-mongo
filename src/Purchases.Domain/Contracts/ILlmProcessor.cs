namespace Purchases.Domain.Contracts;

public interface ILlmProcessor
{
    Task<string> Analyze(string promptMessage, BinaryData binaryData);
    Task<string> Analyze(string systemPrompt, string userMessage);
}
