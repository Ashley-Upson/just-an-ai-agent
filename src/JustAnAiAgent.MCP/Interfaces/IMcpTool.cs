using JustAnAiAgent.MCP.MCP;

namespace JustAnAiAgent.MCP.Interfaces;

public interface IMcpTool
{
    string Name { get; }

    IEnumerable<ToolParameter> GetParameters();

    ToolDefinition GetToolDefinition();

    ValueTask<string> Execute(IEnumerable<ToolParameterInput> parameters);
}