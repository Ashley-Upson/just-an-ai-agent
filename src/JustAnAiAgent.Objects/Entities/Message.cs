namespace JustAnAiAgent.Objects.Entities;

public class Message
{
    public Guid Id { get; set; }

    public Guid ConversationId { get; set; }

    public string? UserId { get; set; }

    public string Type { get; set; }

    public string? ModelId { get; set; }

    public string Content { get; set; }

    public string ContentType { get; set; }

    public DateTimeOffset? ResponseReceivedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public virtual Conversation? Conversation { get; set; }

    public virtual ICollection<AgenticProject>? AgenticProjects { get; set; } 
}