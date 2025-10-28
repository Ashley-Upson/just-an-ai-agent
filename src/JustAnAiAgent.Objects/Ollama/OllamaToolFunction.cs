namespace JustAnAiAgent.Objects.Ollama;

public class OllamaToolFunction
{
    public string name { get; set; }

    public string description { get; set; }

    public OllamaToolFunctionParameters parameters { get; set; }
}
