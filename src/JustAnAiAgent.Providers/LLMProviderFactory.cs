using JustAnAiAgent.Providers.Interfaces;

namespace JustAnAiAgent.Providers;

public class LLMProviderFactory(IEnumerable<IModelProvider> providers) : ILLMProviderFactory
{
    public IModelProvider GetLLMProvider(string name) =>
        providers.FirstOrDefault(p => p.ProviderName == name);
}