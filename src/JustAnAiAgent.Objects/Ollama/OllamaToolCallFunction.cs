using System.Text.Json;

namespace JustAnAiAgent.Objects.Ollama;

public class OllamaToolCallFunction
{
    public string name { get; set; }

    public Dictionary<string, JsonElement> arguments { get; set; }
}
