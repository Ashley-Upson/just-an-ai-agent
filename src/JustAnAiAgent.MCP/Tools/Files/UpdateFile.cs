using JustAnAiAgent.MCP.MCP;
using JustAnAiAgent.MCP.MCP.Files;

namespace JustAnAiAgent.MCP.Tools.Files;

public class UpdateFile(FileHandler fileHandler) : FileToolBase(fileHandler)
{
    public override string Name => "file-update";

    protected override string Description => "Update a text file in the current tool workspace. Paths must be relative.";

    protected override ToolParameters Parameters { get; } = new()
    {
        Type = "object",
        Properties =
        [
            Parameter("path", "string", "Relative path of the file to update.", true),
            Parameter("content", "string", "Replacement text content.", true),
            Parameter("mode", "string", "Update mode: overwrite, prepend, append, insert, or replaceLines. Defaults to overwrite.", false),
            Parameter("startLine", "number", "1-based start line for insert and replaceLines modes.", false),
            Parameter("endLine", "number", "1-based inclusive end line for replaceLines mode.", false),
            Parameter("createIfMissing", "boolean", "Whether to create the file if it does not exist.", false)
        ],
        Required = ["path", "content"]
    };

    public override async ValueTask<string> Execute(IEnumerable<ToolParameterInput> parameters, ToolExecutionContext context)
    {
        return await ExecuteFileOperation(async () =>
        {
            Dictionary<string, object> values = ToParameterDictionary(parameters);
            return await FileHandler.UpdateFileAsync(
                context,
                GetRequiredString(values, "path"),
                GetRequiredString(values, "content"),
                FileUpdateModeParser.Parse(GetOptionalString(values, "mode")),
                GetOptionalBoolean(values, "createIfMissing"),
                GetOptionalInteger(values, "startLine"),
                GetOptionalInteger(values, "endLine"));
        });
    }
}
