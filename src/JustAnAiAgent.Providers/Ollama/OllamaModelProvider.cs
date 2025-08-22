using JustAnAiAgent.Objects.Entities;
using JustAnAiAgent.Objects.Ollama;
using JustAnAiAgent.Providers.Interfaces;
using JustAnAiAjent.Objects.Providers;

namespace JustAnAiAgent.Providers.Ollama;

public class OllamaModelProvider : IModelProvider
{
    private OllamaClient Client {  get; set; }

    public OllamaModelProvider(string apiUrl, int port)
    {
        Client = new OllamaClient(apiUrl, port);
    }

    public async ValueTask<ProviderChatResponse> SendConversationToModelAsync(string model, Conversation conversation)
    {
        ProviderChatRequest request = new (model, conversation);

        OllamaResponse response = await Client.SendChatMessageAsync(request);

        return ProviderResponseFromOllamaResponse(response);
    }

    private ProviderChatResponse ProviderResponseFromOllamaResponse(OllamaResponse response)
    {
        ProviderChatResponse providerResponse = new();
        providerResponse.model = response.model;
        providerResponse.message = response.message.content;
        providerResponse.thought = response.message.thinking;

        return providerResponse;
    }
}