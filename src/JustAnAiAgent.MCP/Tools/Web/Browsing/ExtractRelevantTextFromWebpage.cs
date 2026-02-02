using System.ComponentModel.DataAnnotations;
using HtmlAgilityPack;
using JustAnAiAgent.MCP.MCP;
using JustAnAiAgent.MCP.Interfaces;
using System.Text.Json;

namespace JustAnAiAgent.MCP.Tools.Web.Browsing;

public class ExtractRelevantTextFromWebpage : IMcpTool
{
    public string Name => "extract-relevant-text-from-webpage";

    private ToolParameters Parameters = new()
    {
        Type = "object",
        Properties = new List<ToolParameterProperty> {
            new() {
                Name = "url",
                Type = "string",
                Description = "The full URL of the page to retrieve text from.",
                Required = true,
            },
            new()
            {
                Name = "topic",
                Type = "string",
                Description = "The topic or user query to filter text with.",
                Required = true
            }
        },
        Required = new List<string> { "url", "topic" },
    };

    public ToolParameters GetParameters() =>
        Parameters;

    public ToolDefinition GetToolDefinition()
    {
        return new()
        {
            Name = Name,
            Description = "Extract relevant text from a given webpage.",
            Type = "function",
            Parameters = Parameters,
            Required = Parameters.Properties.Where(p => p.Required).Select(p => p.Name).ToArray()
        };
    }

    public async ValueTask<string> Execute(IEnumerable<ToolParameterInput> parameters)
    {

        var domParameter = parameters.FirstOrDefault(p => p.Name == "dom");

        if (domParameter is null)
            throw new ValidationException("Parameter 'dom' is required.");

        IEnumerable<string> text = new List<string>();

        var document = new HtmlDocument();
        document.LoadHtml(domParameter.Value);

        return JsonSerializer.Serialize(new
        {
            TextContent = string.Join('\n', text)
        });
    }
}