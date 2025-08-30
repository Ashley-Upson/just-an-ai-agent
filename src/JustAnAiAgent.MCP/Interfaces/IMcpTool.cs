using JustAnAiAgent.MCP.MCP;

namespace JustAnAiAgent.MCP.Interfaces;

public interface IMcpTool
{
    IEnumerable<ToolParameter> GetParameters();

    ToolDefinition GetToolDefinition();

    string Execute(IEnumerable<ToolParameterInput> parameters);
}