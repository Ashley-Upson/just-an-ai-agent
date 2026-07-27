using JustAnAiAgent.MCP.MCP;

namespace JustAnAiAgent.MCP.Tools.Files;

public class ZipFileTool(FileHandler fileHandler) : FileToolBase(fileHandler)
{
    public override string Name => "file-zip";

    protected override string Description => "Create a zip archive from a file or directory in the current tool workspace. Paths must be relative.";

    protected override ToolParameters Parameters { get; } = new()
    {
        Type = "object",
        Properties =
        [
            Parameter("sourcePath", "string", "Relative path of the file or directory to archive.", true),
            Parameter("zipPath", "string", "Relative path of the zip file to create.", true),
            Parameter("overwrite", "boolean", "Whether to overwrite an existing zip file.", false)
        ],
        Required = ["sourcePath", "zipPath"]
    };

    public override async ValueTask<string> Execute(IEnumerable<ToolParameterInput> parameters, ToolExecutionContext context)
    {
        Dictionary<string, object> values = ToParameterDictionary(parameters);
        FileHandlerResult result = await FileHandler.ZipAsync(
            context,
            GetRequiredString(values, "sourcePath"),
            GetRequiredString(values, "zipPath"),
            GetOptionalBoolean(values, "overwrite"));

        return SerializeResult(result);
    }
}
