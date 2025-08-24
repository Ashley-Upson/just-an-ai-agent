namespace JustAnAiAjent.Objects.Ollama;

public class OllamaTagsModelDetails
{
    public string format {  get; set; }

    public string family { get; set; }

    public IEnumerable<string> families { get; set; }

    public string parameter_size { get; set; }

    public string quantization_level { get; set; }
}
