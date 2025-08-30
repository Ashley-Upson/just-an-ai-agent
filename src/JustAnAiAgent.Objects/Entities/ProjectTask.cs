namespace JustAnAiAgent.Objects.Entities;

public class ProjectTask
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }

    public Guid? ConversationId { get; set; }

    public int Order { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public string? AdditionalContext { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? CompletedAt {  get; set; }

    public virtual Conversation? Conversation { get; set; }

    public virtual AgenticProject? AgenticProject { get; set; }
}