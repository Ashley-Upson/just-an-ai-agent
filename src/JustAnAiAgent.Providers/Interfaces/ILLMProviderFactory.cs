namespace JustAnAiAgent.Providers.Interfaces;
public interface ILLMProviderFactory
{
    IModelProvider GetLLMProvider(string name);
}