using JustAnAiAgent.Objects.Entities;
using JustAnAiAgent.Providers;
using JustAnAiAjent.Objects.Providers;

namespace JustAnAiAgent.Services.Foundation.Interfaces;

public interface ILLMProviderService
{
    ValueTask<ProviderChatResponse> SendConversationToModelAsync(string model, Conversation conversation);
}
