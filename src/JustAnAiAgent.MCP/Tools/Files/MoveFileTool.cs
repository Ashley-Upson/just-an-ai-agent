using JustAnAiAgent.MCP.MCP;

namespace JustAnAiAgent.MCP.Tools.Files;

public class MoveFileTool(FileHandler fileHandler) : FileToolBase(fileHandler)
{
    public override string Name => "file-move";

    protected override string Description => "Move a file or directory within the current tool workspace. Paths must be relative.";

    protected override ToolParameters Parameters { get; } = new()
    {
        Type = "object",
        Properties =
        [
            Parameter("sourcePath", "string", "Relative path of the file or directory to move.", true),
            Parameter("destinationPath", "string", "Relative destination path.", true),
            Parameter("overwrite", "boolean", "Whether to overwrite an existing destination.", false)
        ],
        Required = ["sourcePath", "destinationPath"]
    };

    public override async ValueTask<string> Execute(IEnumerable<ToolParameterInput> parameters, ToolExecutionContext context)
    {
        Dictionary<string, object> values = ToParameterDictionary(parameters);
        FileHandlerResult result = await FileHandler.MoveAsync(
            context,
            GetRequiredString(values, "sourcePath"),
            GetRequiredString(values, "destinationPath"),
            GetOptionalBoolean(values, "overwrite"));

        return SerializeResult(result);
    }
}
