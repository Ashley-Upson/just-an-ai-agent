namespace JustAnAiAgent.MCP.MCP.Files;

public class FileHandlerResult
{
    public bool Success { get; set; } = true;

    public string? Content { get; set; }

    public IEnumerable<string>? Paths { get; set; }

    public string? Message { get; set; }
}
