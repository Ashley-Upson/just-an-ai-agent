using JustAnAiAgent.MCP.MCP;
using JustAnAiAgent.MCP.MCP.Files;

namespace JustAnAiAgent.MCP.Tools.Files;

public class CreateFile(FileHandler fileHandler) : FileToolBase(fileHandler)
{
    public override string Name => "file-create";

    protected override string Description => "Create a file in the current tool workspace. Paths must be relative.";

    protected override ToolParameters Parameters { get; } = new()
    {
        Type = "object",
        Properties =
        [
            Parameter("path", "string", "Relative path of the file to create.", true),
            Parameter("content", "string", "Text content to write to the file.", false),
            Parameter("overwrite", "boolean", "Whether to overwrite the file if it already exists.", false)
        ],
        Required = ["path"]
    };

    public override async ValueTask<string> Execute(IEnumerable<ToolParameterInput> parameters, ToolExecutionContext context)
    {
        return await ExecuteFileOperation(async () =>
        {
            Dictionary<string, object> values = ToParameterDictionary(parameters);
            return await FileHandler.CreateFileAsync(
                context,
                GetRequiredString(values, "path"),
                GetOptionalString(values, "content") ?? string.Empty,
                GetOptionalBoolean(values, "overwrite"));
        });
    }
}
