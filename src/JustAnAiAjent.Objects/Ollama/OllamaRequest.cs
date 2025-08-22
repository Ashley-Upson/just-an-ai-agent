namespace JustAnAiAgent.Objects.Ollama;

public class OllamaRequest
{
    public string model {  get; set; }

    public ICollection<OllamaMessage> messages { get; set; }

    public bool stream {  get; set; }
}
