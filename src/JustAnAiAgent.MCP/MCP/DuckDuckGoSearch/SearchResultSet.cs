namespace JustAnAiAgent.MCP.MCP.DuckDuckGoSearch;

public class SearchResultSet
{
    public IEnumerable<SearchResult> Results { get; set; }

    public Dictionary<string, dynamic>? PreviousPageRequestBody { get; set; }

    public Dictionary<string, dynamic>? NextPageRequestBody { get; set; }
}
