namespace JustAnAiAgent.Objects.Ollama;

public class OllamaToolFunctionParameters
{
    public string type { get; set; }

    public Dictionary<string, OllamaToolFunctionParameter> properties { get; set; }

    public IEnumerable<string> required { get; set; }
}