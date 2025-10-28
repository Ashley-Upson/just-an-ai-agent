namespace JustAnAiAgent.Objects.Ollama;

public class OllamaToolDefinition
{
    public string type { get; set; }

    public OllamaToolFunction function { get; set; }
}