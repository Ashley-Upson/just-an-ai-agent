namespace JustAnAiAgent.Objects.Entities;

public class Conversation
{
    public Guid Id { get; set; }

    public Guid? ProjectId {  get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public DateTimeOffset? LastMessageSentAt { get; set; }

    public virtual AgenticProject? AgenticProject { get; set; }

    public virtual ICollection<Message>? Messages { get; set; }

    public virtual ICollection<UserConversation>? Users { get; set; } = new List<UserConversation>();
}