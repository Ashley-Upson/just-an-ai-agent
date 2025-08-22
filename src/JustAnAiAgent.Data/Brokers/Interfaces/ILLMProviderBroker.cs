using JustAnAiAgent.Objects.Entities;
using JustAnAiAjent.Objects.Providers;

namespace JustAnAiAgent.Data.Brokers.Interfaces;

public interface ILLMProviderBroker
{
    ValueTask<ProviderChatResponse> SendConversationToModelAsync(string model, Conversation conversation);
}
