using JustAnAiAgent.MCP.MCP;
using JustAnAiAgent.MCP.MCP.Files;

namespace JustAnAiAgent.MCP.Tools.Files;

public class RenameFile(FileHandler fileHandler) : FileToolBase(fileHandler)
{
    public override string Name => "file-rename";

    protected override string Description => "Rename a file or directory in the current tool workspace. Paths must be relative.";

    protected override ToolParameters Parameters { get; } = new()
    {
        Type = "object",
        Properties =
        [
            Parameter("path", "string", "Relative path of the file or directory to rename.", true),
            Parameter("newName", "string", "New file or directory name. This must not be a path.", true),
            Parameter("overwrite", "boolean", "Whether to overwrite an existing destination.", false)
        ],
        Required = ["path", "newName"]
    };

    public override async ValueTask<string> Execute(IEnumerable<ToolParameterInput> parameters, ToolExecutionContext context)
    {
        return await ExecuteFileOperation(async () =>
        {
            Dictionary<string, object> values = ToParameterDictionary(parameters);
            return await FileHandler.RenameAsync(
                context,
                GetRequiredString(values, "path"),
                GetRequiredString(values, "newName"),
                GetOptionalBoolean(values, "overwrite"));
        });
    }
}
