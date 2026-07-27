using JustAnAiAgent.MCP.MCP;

namespace JustAnAiAgent.MCP.Interfaces;

public interface IMcpTool
{
    string Name { get; }

    ToolParameters GetParameters();

    ToolDefinition GetToolDefinition();

    ValueTask<string> Execute(IEnumerable<ToolParameterInput> parameters);

    ValueTask<string> Execute(IEnumerable<ToolParameterInput> parameters, ToolExecutionContext context) =>
        Execute(parameters);
}
