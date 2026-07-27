namespace JustAnAiAgent.Providers.Interfaces;

public interface IModelProviderFactory
{
    IModelProvider CreateProvider(string providerName);

    IModelProvider CreateProviderForModel(string modelId);

    IEnumerable<IModelProvider> CreateAllProviders();
}
