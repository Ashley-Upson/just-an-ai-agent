namespace JustAnAiAgent.Objects.Entities;

public class UserConversation
{
    public string UserId { get; set; }

    public Guid ConversationId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public virtual Conversation? Conversation { get; set; }
}