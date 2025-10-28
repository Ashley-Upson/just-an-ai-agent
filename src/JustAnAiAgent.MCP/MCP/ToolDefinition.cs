namespace JustAnAiAgent.MCP.MCP;

public class  ToolDefinition
{
    public string Name { get; set; }

    public string Type { get; set; } = "function";

    public string Description { get; set; }

    public ToolParameters Parameters { get; set; }

    public IEnumerable<string> Required { get; set; }
}