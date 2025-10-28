using System.ComponentModel.DataAnnotations;
using HtmlAgilityPack;
using JustAnAiAgent.MCP.MCP;
using JustAnAiAgent.MCP.Interfaces;
using System.Text.Json;

namespace JustAnAiAgent.MCP.Tools.Web.Browsing;

public class ExtractTextContentFromDom : IMcpTool
{
    public string Name => "extract-text-content-from-dom";

    private ToolParameters Parameters = new()
    {
        Type = "object",
        Properties = new List<ToolParameterProperty> {
            new() {
                Name = "dom",
                Type = "string",
                Description = "The HTML DOM to extract text nodes from.",
                Required = true,
            }
        },
        Required = new List<string> { "dom" },
    };

    public ToolParameters GetParameters() =>
        Parameters;

    public ToolDefinition GetToolDefinition()
    {
        return new()
        {
            Name = Name,
            Description = "Extract all text content from the given HTML DOM.",
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