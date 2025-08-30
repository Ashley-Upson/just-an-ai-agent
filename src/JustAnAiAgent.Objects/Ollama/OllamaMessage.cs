namespace JustAnAiAgent.Objects.Ollama;

public class OllamaMessage
{
    public string role {  get; set; }

    public string content { get; set; }

    public string thinking { get; set; }

    public ICollection<OllamaToolCall> tool_calls { get; set; }
}
