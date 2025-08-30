using JustAnAiAgent.MCP.Interfaces;

namespace JustAnAiAgent.MCP.MCP;

public abstract class McpTool : IMcpTool
{
    public string Name;

    public abstract IEnumerable<ToolParameter> GetParameters();

    public abstract ToolDefinition GetToolDefinition();

    public abstract string Execute(IEnumerable<ToolParameterInput> parameters);
}
