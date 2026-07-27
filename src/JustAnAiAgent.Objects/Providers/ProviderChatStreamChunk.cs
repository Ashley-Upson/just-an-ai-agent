namespace JustAnAiAgent.Objects.Providers;

public class ProviderChatStreamChunk
{
    public string model { get; set; }

    public string thought { get; set; }

    public string message { get; set; }

    public dynamic tool_calls { get; set; }

    public bool done { get; set; }
}
