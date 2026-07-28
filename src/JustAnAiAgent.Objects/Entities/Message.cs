using System.Text.Json.Serialization;

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

    public bool IsComplete { get; set; }

    public bool IsStillRunning { get; set; }

    public int? TokenUsage { get; set; }

    [JsonIgnore]
    public virtual Conversation? Conversation { get; set; }

    [JsonIgnore]
    public virtual ICollection<AgenticProject>? AgenticProjects { get; set; } 
}
