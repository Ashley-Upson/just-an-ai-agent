using JustAnAiAgent.MCP.MCP;
using JustAnAiAgent.Objects.Entities;
using JustAnAiAjent.Objects.Providers;

namespace JustAnAiAgent.Providers.Interfaces;

public interface IModelProvider
{
    string ProviderName { get; }

    ValueTask<string[]> GetAvailableModelsAsync();

    ValueTask<ProviderChatResponse> SendConversationToModelAsync(string model, Conversation conversation);

    ValueTask<ProviderChatResponse> SendConversationToModelWithToolsAsync(string model, Conversation conversation, IEnumerable<ToolDefinition> tools);
}
