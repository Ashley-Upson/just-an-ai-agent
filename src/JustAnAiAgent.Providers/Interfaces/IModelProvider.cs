using JustAnAiAgent.Objects.Entities;
using JustAnAiAjent.Objects.Providers;

namespace JustAnAiAgent.Providers.Interfaces;

public interface IModelProvider
{
    ValueTask<ProviderChatResponse> SendConversationToModelAsync(string model, Conversation conversation);
}
