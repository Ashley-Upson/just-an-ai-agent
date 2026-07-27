using JustAnAiAgent.MCP.MCP;
using JustAnAiAgent.MCP.MCP.Files;

namespace JustAnAiAgent.MCP.Tools.Files;

public class UnzipFile(FileHandler fileHandler) : FileToolBase(fileHandler)
{
    public override string Name => "file-unzip";

    protected override string Description => "Extract a zip archive into the current tool workspace. Paths must be relative.";

    protected override ToolParameters Parameters { get; } = new()
    {
        Type = "object",
        Properties =
        [
            Parameter("zipPath", "string", "Relative path of the zip file to extract.", true),
            Parameter("destinationPath", "string", "Relative destination directory.", true),
            Parameter("overwrite", "boolean", "Whether to overwrite extracted files.", false)
        ],
        Required = ["zipPath", "destinationPath"]
    };

    public override async ValueTask<string> Execute(IEnumerable<ToolParameterInput> parameters, ToolExecutionContext context)
    {
        Dictionary<string, object> values = ToParameterDictionary(parameters);
        FileHandlerResult result = await FileHandler.UnzipAsync(
            context,
            GetRequiredString(values, "zipPath"),
            GetRequiredString(values, "destinationPath"),
            GetOptionalBoolean(values, "overwrite"));

        return SerializeResult(result);
    }
}
