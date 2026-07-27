using JustAnAiAgent.Data.Brokers.Interfaces;
using JustAnAiAgent.MCP.MCP;
using JustAnAiAgent.Objects.Entities;
using JustAnAiAgent.Services.Foundation.Interfaces;
using JustAnAiAgent.Objects.Providers;

namespace JustAnAiAgent.Services.Foundation;

public class OllamaProviderService(ILLMProviderBroker providerBroker) : ILLMProviderService
{
    public async ValueTask<string[]> GetAvailableModelsAsync() =>
        await providerBroker.GetAvailableModelsAsync();

    public async ValueTask<ProviderChatResponse> SendConversationToModelAsync(string model, Conversation conversation) =>
        await providerBroker.SendConversationToModelAsync(model, conversation);

    public async ValueTask<ProviderChatResponse> SendConversationToModelWithToolsAsync(string model, Conversation conversation, IEnumerable<ToolDefinition> tools) =>
        await providerBroker.SendConversationToModelWithToolsAsync(model, conversation, tools);

    public async IAsyncEnumerable<ProviderChatStreamChunk> SendConversationToModelWithToolsStreamAsync(
        string model,
        Conversation conversation,
        IEnumerable<ToolDefinition> tools,
        CancellationToken cancellationToken = default)
    {
        await foreach (ProviderChatStreamChunk chunk in providerBroker.SendConversationToModelWithToolsStreamAsync(model, conversation, tools, cancellationToken))
            yield return chunk;
    }
}
