using JustAnAiAgent.Providers.Interfaces;
using JustAnAiAgent.Providers.Ollama;
using Microsoft.Extensions.Configuration;

namespace JustAnAiAgent.Providers.Factories;

public class ModelProviderFactory(IConfiguration configuration) : IModelProviderFactory
{
    private const string DefaultOllamaApiUrl = "http://192.168.1.127";
    private const int DefaultOllamaPort = 11434;

    public IModelProvider CreateProvider(string providerName)
    {
        if (string.IsNullOrWhiteSpace(providerName))
            throw new ArgumentException("Provider name must be provided.", nameof(providerName));

        // Todo: hook this into DI registered providers.
        return providerName.Trim().ToLowerInvariant() switch
        {
            "ollama" => CreateOllamaProvider(),
            _ => throw new NotSupportedException($"Provider '{providerName}' is not registered.")
        };
    }

    public IModelProvider CreateProviderForModel(string modelId)
    {
        string providerName = ExtractProviderName(modelId) ?? "Ollama";
        return CreateProvider(providerName);
    }

    public IEnumerable<IModelProvider> CreateAllProviders() =>
        new[] { CreateOllamaProvider() };

    private OllamaModelProvider CreateOllamaProvider()
    {
        // To do: this needs to come from user providers.
        string apiUrl = configuration["LLMProviders:Ollama:ApiUrl"] ?? DefaultOllamaApiUrl;
        int port = configuration.GetValue<int?>("LLMProviders:Ollama:Port") ?? DefaultOllamaPort;

        return new OllamaModelProvider(apiUrl, port);
    }

    private static string? ExtractProviderName(string modelId)
    {
        if (string.IsNullOrWhiteSpace(modelId))
            return null;

        int start = modelId.IndexOf('<');
        int end = modelId.IndexOf('>');

        if (start == 0 && end > 1)
            return modelId.Substring(1, end - 1);

        return null;
    }
}
