using JustAnAiAgent.Objects.Ollama;

namespace JustAnAiAgent.Providers.Ollama.Brokers.Interfaces;

public interface IOllamaBroker
{
    ValueTask<OllamaTagsResponse> GetAvailableModelsAsync();

    ValueTask<OllamaResponse> SendMessageAsync();

    ValueTask<OllamaResponse> SendMessageWithToolsAsync();
}