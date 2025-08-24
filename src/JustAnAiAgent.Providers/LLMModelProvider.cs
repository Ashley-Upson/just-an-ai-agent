using JustAnAiAgent.Objects.Entities;
using JustAnAiAgent.Providers.Interfaces;
using JustAnAiAjent.Objects.Providers;

namespace JustAnAiAgent.Providers;

public abstract class LLMModelProvider : IModelProvider
{
    public string providerName { get; }

    protected LLMModelProvider()
    {
        providerName = $"{GetType().Name.Replace("ModelProvider", "")}";
    }

    public abstract ValueTask<string[]> GetAvailableModelsAsync();

    public abstract ValueTask<ProviderChatResponse> SendConversationToModelAsync(string model, Conversation conversation);
}