using JustAnAiAgent.MCP.DirectoryServices;
using JustAnAiAgent.MCP.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace JustAnAiAgent.Services;

public static class IServiceCollectionExtensions
{
    public static void AddMcpTools(this IServiceCollection services)
    {
        services.AddTransient<IMcpTool, GetDirectoryTree>();
    }
}