using cCoder.Security.Objects.Entities;

namespace JustAnAiAgent.Data.Entities;

public class UserConversation
{
    public string UserId { get; set; }

    public Guid ConversationId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public virtual SSOUser User { get; set; }

    public virtual Conversation Conversation { get; set; }
}
