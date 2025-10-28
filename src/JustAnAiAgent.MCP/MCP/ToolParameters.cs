namespace JustAnAiAgent.MCP.MCP;

public class ToolParameters
{
    public string Type { get; set; }

    public IEnumerable<string> Required { get; set; }

    public IEnumerable<ToolParameterProperty> Properties { get; set; }
}