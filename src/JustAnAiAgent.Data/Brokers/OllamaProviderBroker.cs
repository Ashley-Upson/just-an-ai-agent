using JustAnAiAgent.Data.Brokers.Interfaces;
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
        await this.ollamaProvider.GetAvailableModelsAsync();

    public async ValueTask<ProviderChatResponse> SendConversationToModelAsync(string model, Conversation conversation) =>
        await this.ollamaProvider.SendConversationToModelAsync(model, conversation);
}