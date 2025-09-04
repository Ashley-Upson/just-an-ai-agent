using JustAnAiAgent.MCP.Interfaces;
using JustAnAiAgent.MCP.Tools.DirectoryServices;
using JustAnAiAgent.MCP.Tools.Web.Search;
using Microsoft.Extensions.DependencyInjection;

namespace JustAnAiAgent.Services;

public static class IServiceCollectionExtensions
{
    public static void AddMcpTools(this IServiceCollection services)
    {
        services.AddTransient<IMcpTool, GetDirectoryTree>();
        services.AddTransient<IMcpTool, DuckDuckGoSearch>();
    }
}