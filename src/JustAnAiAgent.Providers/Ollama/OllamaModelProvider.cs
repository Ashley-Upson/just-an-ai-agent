using JustAnAiAgent.MCP.MCP;
using JustAnAiAgent.Objects.Entities;
using JustAnAiAgent.Objects.Ollama;
using JustAnAiAgent.Providers.Interfaces;
using JustAnAiAjent.Objects.Ollama;
using JustAnAiAjent.Objects.Providers;

namespace JustAnAiAgent.Providers.Ollama;

public class OllamaModelProvider : IModelProvider
{
    public string ProviderName => "Ollama";

    private OllamaClient client {  get; set; }

    public OllamaModelProvider(string apiUrl, int port)
    {
        client = new OllamaClient(apiUrl, port);
    }

    public async ValueTask<string[]> GetAvailableModelsAsync()
    {
        OllamaTagsResponse response = await client.GetAvailableModelsAsync();
        return response.models
            .Select(m => $"<{ProviderName}>{m.name}")
            .ToArray();
    }

    public async ValueTask<ProviderChatResponse> SendConversationToModelAsync(string model, Conversation conversation)
    {
        ProviderChatRequest request = new(model, conversation);

        OllamaResponse response = await client.SendChatMessageAsync(request);

        return ProviderResponseFromOllamaResponse(response);
    }

    public async ValueTask<ProviderChatResponse> SendConversationToModelWithToolsAsync(string model, Conversation conversation, IEnumerable<ToolDefinition> tools)
    {
        ProviderChatRequest request = new(model, conversation);

        OllamaResponse response = await client.SendChatMessageWithToolsAsync(request, tools);

        return ProviderResponseFromOllamaResponse(response);
    }

    private ProviderChatResponse ProviderResponseFromOllamaResponse(OllamaResponse response)
    {
        ProviderChatResponse providerResponse = new();
        providerResponse.model = response.model;
        providerResponse.message = response.message.content;
        providerResponse.thought = response.message.thinking;
        providerResponse.tool_calls = response.message.tool_calls;

        return providerResponse;
    }
}