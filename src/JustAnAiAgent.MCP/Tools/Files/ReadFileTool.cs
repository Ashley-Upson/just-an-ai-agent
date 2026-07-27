using JustAnAiAgent.MCP.MCP;

namespace JustAnAiAgent.MCP.Tools.Files;

public class ReadFileTool(FileHandler fileHandler) : FileToolBase(fileHandler)
{
    public override string Name => "file-read";

    protected override string Description => "Read a text file from the current tool workspace. Paths must be relative.";

    protected override ToolParameters Parameters { get; } = new()
    {
        Type = "object",
        Properties =
        [
            Parameter("path", "string", "Relative path of the file to read.", true)
        ],
        Required = ["path"]
    };

    public override async ValueTask<string> Execute(IEnumerable<ToolParameterInput> parameters, ToolExecutionContext context)
    {
        Dictionary<string, object> values = ToParameterDictionary(parameters);
        FileHandlerResult result = await FileHandler.ReadFileAsync(context, GetRequiredString(values, "path"));

        return SerializeResult(result);
    }
}
