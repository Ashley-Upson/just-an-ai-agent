namespace JustAnAiAgent.Objects.Ollama;

public class OllamaMessage
{
    public string role {  get; set; }

    public string content { get; set; }

    public string thinking { get; set; }

    public string tool_name { get; set; }

    public IEnumerable<OllamaToolCall> tool_calls { get; set; }

    public IEnumerable<string> images { get; set; }
}
