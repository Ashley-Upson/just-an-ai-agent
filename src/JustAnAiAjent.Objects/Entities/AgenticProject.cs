namespace JustAnAiAgent.Objects.Entities;

public class AgenticProject
{
    public Guid Id { get; set; }

    public Guid MessageId { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }

    public string? AdditionalContext { get; set; }

    public string? Goal { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public virtual Message? Message { get; set; }

    public virtual ICollection<Conversation>? Conversations { get; set; }

    public virtual ICollection<ProjectTask>? ProjectTasks { get; set; }
}