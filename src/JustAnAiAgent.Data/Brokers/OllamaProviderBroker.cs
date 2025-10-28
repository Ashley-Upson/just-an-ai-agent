using JustAnAiAgent.Data.Brokers.Interfaces;
using JustAnAiAgent.MCP.MCP;
using JustAnAiAgent.Objects.Entities;
using JustAnAiAgent.Providers.Ollama;
using JustAnAiAjent.Objects.Providers;

namespace JustAnAiAgent.Data.Brokers;

public class OllamaProviderBroker : ILLMProviderBroker
{
    private readonly OllamaModelProvider ollamaProvider;

    public OllamaProviderBroker() =>
        this.ollamaProvider = new OllamaModelProvider("http://192.168.1.127", 11434);

    public async ValueTask<string[]> GetAvailableModelsAsync() =>
        await ollamaProvider.GetAvailableModelsAsync();

    public async ValueTask<ProviderChatResponse> SendConversationToModelAsync(string model, Conversation conversation) =>
        await ollamaProvider.SendConversationToModelAsync(model, conversation);

    public async ValueTask<ProviderChatResponse> SendConversationToModelWithToolsAsync(string model, Conversation conversation, IEnumerable<ToolDefinition> tools) =>
        await ollamaProvider.SendConversationToModelWithToolsAsync(model, conversation, tools);
}