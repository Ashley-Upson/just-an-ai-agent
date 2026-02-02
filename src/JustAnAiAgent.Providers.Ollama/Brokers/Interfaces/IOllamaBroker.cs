using JustAnAiAgent.Providers.Ollama.Objects;

namespace JustAnAiAgent.Providers.Ollama.Brokers.Interfaces;

public interface IOllamaBroker
{
    ValueTask<OllamaTagsResponse> GetAvailableModelsAsync();

    ValueTask<OllamaResponse> SendMessageAsync();

    ValueTask<OllamaResponse> SendMessageWithToolsAsync();
}
