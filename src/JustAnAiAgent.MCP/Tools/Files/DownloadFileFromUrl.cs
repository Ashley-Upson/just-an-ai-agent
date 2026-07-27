using JustAnAiAgent.MCP.MCP;
using JustAnAiAgent.MCP.MCP.Files;

namespace JustAnAiAgent.MCP.Tools.Files;

public class DownloadFileFromUrl(FileHandler fileHandler) : FileToolBase(fileHandler)
{
    public override string Name => "file-download-from-url";

    protected override string Description => "Download a file from an HTTP or HTTPS URL into the current tool workspace. Paths must be relative.";

    protected override ToolParameters Parameters { get; } = new()
    {
        Type = "object",
        Properties =
        [
            Parameter("url", "string", "HTTP or HTTPS URL to download.", true),
            Parameter("path", "string", "Relative path where the downloaded file should be saved.", true),
            Parameter("overwrite", "boolean", "Whether to overwrite the file if it already exists.", false)
        ],
        Required = ["url", "path"]
    };

    public override async ValueTask<string> Execute(IEnumerable<ToolParameterInput> parameters, ToolExecutionContext context)
    {
        return await ExecuteFileOperation(async () =>
        {
            Dictionary<string, object> values = ToParameterDictionary(parameters);
            return await FileHandler.DownloadFileFromUrlAsync(
                context,
                GetRequiredString(values, "url"),
                GetRequiredString(values, "path"),
                GetOptionalBoolean(values, "overwrite"));
        });
    }
}
