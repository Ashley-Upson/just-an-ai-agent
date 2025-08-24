using System.Net.Http.Json;
using JustAnAiAgent.Objects.Entities;
using JustAnAiAgent.Objects.Ollama;
using JustAnAiAjent.Objects.Ollama;
using JustAnAiAjent.Objects.Providers;

namespace JustAnAiAgent.Providers.Ollama;

class OllamaClient
{
    private string ApiUrl {  get; set; }

    private int Port { get; set; }

    private HttpClient ApiClient { get; set; }

    public OllamaClient(string url, int port)
    {
        ApiUrl = url;
        Port = port;

        ApiClient = new()
        {
            BaseAddress = new Uri($"{ApiUrl}:{Port}/api/"),
            Timeout = Timeout.InfiniteTimeSpan
        };
    }

    public async ValueTask<OllamaResponse> SendChatMessageAsync(ProviderChatRequest request)
    {
        OllamaRequest ollamaRequest = BuildBasicChatRequest(request);

        HttpResponseMessage httpResponse = await ApiClient.PostAsJsonAsync("chat", ollamaRequest);
        httpResponse.EnsureSuccessStatusCode();
        OllamaResponse response = await httpResponse.Content.ReadFromJsonAsync<OllamaResponse>();

        return response;
    }

    public async ValueTask<OllamaTagsResponse> GetAvailableModelsAsync()
    {
        HttpResponseMessage httpResponse = await ApiClient.GetAsync("tags");
        httpResponse.EnsureSuccessStatusCode();
        return await httpResponse.Content.ReadFromJsonAsync<OllamaTagsResponse>();
    }

    private OllamaRequest BuildBasicChatRequest(ProviderChatRequest request)
    {
        OllamaRequest ollamaRequest = new();
        ollamaRequest.stream = false;
        List<OllamaMessage> messages = new();

        foreach (var message in request.messages)
            messages.AddRange(OllamaMessagesFromMessage(message));

        ollamaRequest.messages = messages;
        ollamaRequest.model = request.model;
        return ollamaRequest;
    }

    private IEnumerable<OllamaMessage> OllamaMessagesFromMessage(Message message)
    {
        List<OllamaMessage> messages = new();
        
        OllamaMessage userMessage = new();
        userMessage.role = "user";
        userMessage.content = message.SystemPrompt ?? message.UserPrompt;
        messages.Add(userMessage);

        if (message.ResponseReceivedAt is not null)
        {
            OllamaMessage modelResponse = new();
            modelResponse.role = "assistant";
            modelResponse.content = message.ModelResponse;
            messages.Add(modelResponse);
        }

        return messages;
    }
}