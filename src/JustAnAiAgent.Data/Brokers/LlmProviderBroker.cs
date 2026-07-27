using JustAnAiAgent.Data.Brokers.Interfaces;
using JustAnAiAgent.MCP.MCP;
using JustAnAiAgent.Objects.Entities;
using JustAnAiAgent.Objects.Providers;
using JustAnAiAgent.Providers.Interfaces;

namespace JustAnAiAgent.Data.Brokers;

public class LlmProviderBroker(IModelProviderFactory providerFactory) : ILLMProviderBroker
{
    public async ValueTask<string[]> GetAvailableModelsAsync()
    {
        IEnumerable<IModelProvider> providers = providerFactory.CreateAllProviders();
        string[][] models = await Task.WhenAll(providers.Select(p => p.GetAvailableModelsAsync().AsTask()));

        return models.SelectMany(model => model)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public async ValueTask<ProviderChatResponse> SendConversationToModelAsync(string model, Conversation conversation)
    {
        IModelProvider provider = providerFactory.CreateProviderForModel(model);
        string modelName = NormalizeModelName(model, provider.ProviderName);

        return await provider.SendConversationToModelAsync(modelName, conversation);
    }

    public async ValueTask<ProviderChatResponse> SendConversationToModelWithToolsAsync(string model, Conversation conversation, IEnumerable<ToolDefinition> tools)
    {
        IModelProvider provider = providerFactory.CreateProviderForModel(model);
        string modelName = NormalizeModelName(model, provider.ProviderName);

        return await provider.SendConversationToModelWithToolsAsync(modelName, conversation, tools);
    }

    public async IAsyncEnumerable<ProviderChatStreamChunk> SendConversationToModelWithToolsStreamAsync(
        string model,
        Conversation conversation,
        IEnumerable<ToolDefinition> tools,
        CancellationToken cancellationToken = default)
    {
        IModelProvider provider = providerFactory.CreateProviderForModel(model);
        string modelName = NormalizeModelName(model, provider.ProviderName);

        await foreach (ProviderChatStreamChunk chunk in provider.SendConversationToModelWithToolsStreamAsync(modelName, conversation, tools, cancellationToken))
            yield return chunk;
    }

    private static string NormalizeModelName(string model, string providerName)
    {
        if (string.IsNullOrWhiteSpace(model))
            return model;

        string prefix = $"<{providerName}>";

        return model.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? model[prefix.Length..]
            : model;
    }
}
