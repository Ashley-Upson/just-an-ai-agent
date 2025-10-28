namespace JustAnAiAgent.Objects.Ollama;

public class OllamaRequest
{
    public string model {  get; set; }

    public ICollection<OllamaMessage> messages { get; set; }

    public bool stream {  get; set; }

    public IEnumerable<OllamaToolDefinition> tools { get; set; }

    public Dictionary<string, dynamic> options { get; set; }
}