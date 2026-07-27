using JustAnAiAgent.MCP.MCP;
using JustAnAiAgent.MCP.MCP.Files;

namespace JustAnAiAgent.MCP.Tools.Files;

public class DeleteFile(FileHandler fileHandler) : FileToolBase(fileHandler)
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
        return await ExecuteFileOperation(async () =>
        {
            Dictionary<string, object> values = ToParameterDictionary(parameters);
            return await FileHandler.DeleteAsync(
                context,
                GetRequiredString(values, "path"),
                GetOptionalBoolean(values, "recursive"));
        });
    }
}
