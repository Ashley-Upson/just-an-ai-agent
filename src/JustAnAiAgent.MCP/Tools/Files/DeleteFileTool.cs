using JustAnAiAgent.MCP.MCP;

namespace JustAnAiAgent.MCP.Tools.Files;

public class DeleteFileTool(FileHandler fileHandler) : FileToolBase(fileHandler)
{
    public override string Name => "file-delete";

    protected override string Description => "Delete a file or directory from the current tool workspace. Paths must be relative.";

    protected override ToolParameters Parameters { get; } = new()
    {
        Type = "object",
        Properties =
        [
            Parameter("path", "string", "Relative path of the file or directory to delete.", true),
            Parameter("recursive", "boolean", "Whether to recursively delete a directory.", false)
        ],
        Required = ["path"]
    };

    public override async ValueTask<string> Execute(IEnumerable<ToolParameterInput> parameters, ToolExecutionContext context)
    {
        Dictionary<string, object> values = ToParameterDictionary(parameters);
        FileHandlerResult result = await FileHandler.DeleteAsync(
            context,
            GetRequiredString(values, "path"),
            GetOptionalBoolean(values, "recursive"));

        return SerializeResult(result);
    }
}
