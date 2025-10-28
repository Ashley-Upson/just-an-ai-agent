using System.ComponentModel.DataAnnotations;
using JustAnAiAgent.MCP.MCP;
using JustAnAiAgent.MCP.Interfaces;
using Microsoft.Playwright;
using System;
using System.Text.Json;

namespace JustAnAiAgent.MCP.Tools.Web.Browsing;

public class GetWebContentFromUrl : IMcpTool
{
    public string Name => "get-web-content-from-url";

    private ToolParameters Parameters = new()
    {
        Type = "object",
        Properties = new List<ToolParameterProperty> {
            new() {
                Name = "url",
                Type = "string",
                Description = "The full URL of the page to retrieve.",
                Required = true,
            }
        },
        Required = new List<string> { "url" },
    };

    public ToolParameters GetParameters() =>
        Parameters;

    public ToolDefinition GetToolDefinition()
    {
        return new()
        {
            Name = Name,
            Description = "Load a webpage and extract the DOM content.",
            Type = "function",
            Parameters = Parameters,
            Required = Parameters.Properties.Where(p => p.Required).Select(p => p.Name).ToArray()
        };
    }

    public async ValueTask<string> Execute(IEnumerable<ToolParameterInput> parameters)
    {
        var urlParameter = parameters.FirstOrDefault(p => p.Name == "url");

        if (urlParameter is null)
            throw new ValidationException("Parameter 'url' is required.");

        try
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });

            var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();

            await page.GotoAsync(urlParameter.Value, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 30000
            });

            string content = await page.ContentAsync();

            return JsonSerializer.Serialize(new
            {
                DomString = content
            });
        }
        catch(Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                Error = ex.Message
            });
        }
    }
}