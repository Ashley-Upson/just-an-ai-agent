using cCoder.Security.Objects.Entities;

namespace JustAnAiAgent.Data.Entities;

public class Message
{
    public Guid Id { get; set; }

    public Guid ConversationId { get; set; }

    public string? UserId { get; set; }

    public string ModelId { get; set; }

    public string UserPrompt { get; set; }

    public string? SystemPrompt { get; set; }

    public string? ModelThought { get; set; }

    public string? ModelResponse { get; set; }

    public DateTimeOffset? ResponseReceivedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public virtual SSOUser? User { get; set; }

    public virtual Conversation Conversation { get; set; }

    public virtual ICollection<AgenticProject> AgenticProjects { get; set; }
}
