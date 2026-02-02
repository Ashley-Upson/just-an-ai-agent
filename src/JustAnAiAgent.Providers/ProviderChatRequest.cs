using JustAnAiAgent.Objects.Entities;

namespace JustAnAiAjent.Objects.Providers;

public class ProviderChatRequest
{
    public string model { get; set; }

    public ICollection<Message> messages { get; set; }

    public ProviderChatRequest(string model, Conversation conversation)
    {
        this.model = model;
        messages = conversation.Messages;
    }
}