using JustAnAiAgent.Data.Brokers;
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
}