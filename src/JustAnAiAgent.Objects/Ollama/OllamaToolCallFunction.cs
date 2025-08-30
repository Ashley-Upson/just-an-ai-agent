namespace JustAnAiAgent.Objects.Ollama;

public class OllamaToolCallFunction
{
    public string name {  get; set; }

    public IDictionary<string, string> parameters { get; set; }
}
