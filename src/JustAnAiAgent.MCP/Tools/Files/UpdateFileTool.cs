using JustAnAiAgent.MCP.MCP;

namespace JustAnAiAgent.MCP.Tools.Files;

public class UpdateFileTool(FileHandler fileHandler) : FileToolBase(fileHandler)
{
    public override string Name => "file-update";

    protected override string Description => "Overwrite a text file in the current tool workspace. Paths must be relative.";

    protected override ToolParameters Parameters { get; } = new()
    {
        Type = "object",
        Properties =
        [
            Parameter("path", "string", "Relative path of the file to update.", true),
            Parameter("content", "string", "Replacement text content.", true),
            Parameter("createIfMissing", "boolean", "Whether to create the file if it does not exist.", false)
        ],
        Required = ["path", "content"]
    };

    public override async ValueTask<string> Execute(IEnumerable<ToolParameterInput> parameters, ToolExecutionContext context)
    {
        Dictionary<string, object> values = ToParameterDictionary(parameters);
        FileHandlerResult result = await FileHandler.UpdateFileAsync(
            context,
            GetRequiredString(values, "path"),
            GetRequiredString(values, "content"),
            GetOptionalBoolean(values, "createIfMissing"));

        return SerializeResult(result);
    }
}
