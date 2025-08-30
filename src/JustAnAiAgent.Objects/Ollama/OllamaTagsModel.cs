namespace JustAnAiAjent.Objects.Ollama;

public class OllamaTagsModel
{
    public string name {  get; set; }

    public DateTimeOffset modified_at { get; set; }

    public long size { get; set; }

    public string digest {  get; set; }

    public OllamaTagsModelDetails details { get; set; }
}
