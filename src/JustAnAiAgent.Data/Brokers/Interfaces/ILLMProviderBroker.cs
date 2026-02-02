using JustAnAiAgent.MCP.MCP;
using JustAnAiAgent.Objects.Entities;
using JustAnAiAgent.Objects.Providers;

namespace JustAnAiAgent.Data.Brokers.Interfaces;

public interface ILLMProviderBroker
{
    ValueTask<string[]> GetAvailableModelsAsync();

    ValueTask<ProviderChatResponse> SendConversationToModelAsync(string model, Conversation conversation);
    ValueTask<ProviderChatResponse> SendConversationToModelWithToolsAsync(string model, Conversation conversation, IEnumerable<ToolDefinition> tools);
}
