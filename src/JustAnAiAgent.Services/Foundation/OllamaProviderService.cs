using JustAnAiAgent.Data.Brokers;
using JustAnAiAgent.MCP.MCP;
using JustAnAiAgent.Objects.Entities;
using JustAnAiAgent.Services.Foundation.Interfaces;
using JustAnAiAjent.Objects.Providers;

namespace JustAnAiAgent.Services.Foundation;

public class OllamaProviderService(OllamaProviderBroker providerBroker) : ILLMProviderService
{
    public async ValueTask<string[]> GetAvailableModelsAsync() =>
        await providerBroker.GetAvailableModelsAsync();

    public async ValueTask<ProviderChatResponse> SendConversationToModelAsync(string model, Conversation conversation) =>
        await providerBroker.SendConversationToModelAsync(model, conversation);

    public async ValueTask<ProviderChatResponse> SendConversationToModelWithToolsAsync(string model, Conversation conversation, IEnumerable<ToolDefinition> tools) =>
        await providerBroker.SendConversationToModelWithToolsAsync(model, conversation, tools);
}