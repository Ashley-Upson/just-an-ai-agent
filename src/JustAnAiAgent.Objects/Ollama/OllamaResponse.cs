namespace JustAnAiAgent.Objects.Ollama;

public class OllamaResponse
{
    public string model { get; set; }

    public DateTimeOffset created_at { get; set; }

    public OllamaMessage message { get; set; }

    public bool done { get; set; }

    public long total_duration { get; set; }

    public long load_duration { get; set; }

    public long prompt_eval_count { get; set; }

    public long prompt_eval_duration { get; set; }

    public long eval_count { get; set; }

    public long eval_duration { get; set; }
}