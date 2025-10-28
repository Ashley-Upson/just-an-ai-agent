using JustAnAiAgent.MCP.MCP;
using JustAnAiAgent.Objects.Entities;
using JustAnAiAjent.Objects.Providers;

namespace JustAnAiAgent.Services.Foundation.Interfaces;

public interface ILLMProviderService
{
    ValueTask<string[]> GetAvailableModelsAsync();

    ValueTask<ProviderChatResponse> SendConversationToModelAsync(string model, Conversation conversation);

    ValueTask<ProviderChatResponse> SendConversationToModelWithToolsAsync(string model, Conversation conversation, IEnumerable<ToolDefinition> tools);
}