namespace JustAnAiAgent.MCP.MCP;

public class ToolParameter
{
    public string Name { get; set; }

    public string Description { get; set; }

    public string Type { get; set; }

    public bool Required { get; set; }

    public IEnumerable<ToolParameterProperty> Properties { get; set; }
}