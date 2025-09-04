using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using HtmlAgilityPack;
using JustAnAiAgent.MCP.Interfaces;
using JustAnAiAgent.MCP.MCP;
using JustAnAiAgent.MCP.MCP.DuckDuckGoSearch;

namespace JustAnAiAgent.MCP.Tools.Web.Search;

public class DuckDuckGoSearch : IMcpTool
{
    public string Name => "duck-duck-go-search";

    private IEnumerable<ToolParameter> Parameters = new List<ToolParameter>()
    {
        new()
        {
            Name = "query",
            Description = "The search term.",
            Type = "string",
            Required = true,
        }
    };

    public IEnumerable<ToolParameter> GetParameters() =>
        Parameters;

    public ToolDefinition GetToolDefinition()
    {
        return new()
        {
            Name = Name,
            Description = "Do a web search using Duck Duck Go",
            Type = "function",
            Parameters = Parameters,
            Required = Parameters.Where(p => p.Required).Select(p => p.Name).ToArray()
        };
    }

    public async ValueTask<string> Execute(IEnumerable<ToolParameterInput> parameters)
    {
        var queryParameter = parameters.FirstOrDefault(p => p.Name == "query");

        if (queryParameter is null)
            throw new ValidationException("Parameter 'query' is required.");

        string dom = await GetSearchResultsDom(queryParameter.Value.ToString());

        var parsed = ParseResponseDom(dom);

        return JsonSerializer.Serialize(parsed);
    }

    private async ValueTask<string> GetSearchResultsDom(string query)
    {
        HttpClient client = new HttpClient()
        {
            BaseAddress = new Uri("https://html.duckduckgo.com/")
        };

        HttpResponseMessage response = await client.PostAsync($"/html?q={query}", null);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    private SearchResultSet ParseResponseDom(string dom)
    {
        var document = new HtmlDocument();
        document.LoadHtml(dom);

        var resultNodes = document.DocumentNode.SelectNodes("//div[@class='results']//div[@class='result results_links results_links_deep web-result ']//div[@class='links_main links_deep result__body']");
        var pageNodes = document.DocumentNode.SelectNodes("//div[@id='links']/div[@class='nav-link']/form");
        var previousPageBody = new Dictionary<string, dynamic>();
        var nextPageBody = new Dictionary<string, dynamic>();

        var results = new List<SearchResult>();

        foreach(var node in resultNodes)
        {
            results.Add(new()
            {
                Title = node.SelectSingleNode("h2/a[@class='result__a']").InnerText,
                Url = node.SelectSingleNode("a[@class='result__snippet']").GetAttributeValue("href", ""),
                Description = node.SelectSingleNode("a[@class='result__snippet']").InnerText
            });
        }

        foreach(var node in pageNodes)
        {
            string submitText = node.SelectSingleNode("input[@type='submit']").GetAttributeValue("value", "");
            var inputNodes = node.SelectNodes("input[@type='hidden']");
            Dictionary<string, dynamic> body = new();

            foreach(var input in inputNodes)
                body.Add(input.GetAttributeValue("name", ""), input.GetAttributeValue("value", ""));

            if (submitText == "Next")
                nextPageBody = body;
            else
                previousPageBody = body;
        }

        return new SearchResultSet()
        {
            Results = results,
            PreviousPageRequestBody = previousPageBody,
            NextPageRequestBody = nextPageBody
        };
    }
}