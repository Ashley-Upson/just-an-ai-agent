using JustAnAiAgent.MCP.Interfaces;
using JustAnAiAgent.MCP.Tools.DirectoryServices;
using JustAnAiAgent.MCP.Tools.Files;
using JustAnAiAgent.MCP.Tools.PowerShell;
using JustAnAiAgent.MCP.Tools.Web.Browsing;
using JustAnAiAgent.MCP.Tools.Web.Search;
using Microsoft.Extensions.DependencyInjection;

namespace JustAnAiAgent.MCP;

public static class IServiceCollectionExtensions
{
    public static void AddMcpTools(this IServiceCollection services)
    {
        services.AddTransient<FileHandler>();
        services.AddTransient<IMcpTool, GetDirectoryTree>();
        services.AddTransient<IMcpTool, RunPowerShellCommand>();
        services.AddTransient<IMcpTool, CreateFileTool>();
        services.AddTransient<IMcpTool, ReadFileTool>();
        services.AddTransient<IMcpTool, UpdateFileTool>();
        services.AddTransient<IMcpTool, DeleteFileTool>();
        services.AddTransient<IMcpTool, RenameFileTool>();
        services.AddTransient<IMcpTool, MoveFileTool>();
        services.AddTransient<IMcpTool, ZipFileTool>();
        services.AddTransient<IMcpTool, UnzipFileTool>();
        services.AddTransient<IMcpTool, DuckDuckGoSearch>();
        services.AddTransient<IMcpTool, GetWebContentFromUrl>();
        services.AddTransient<IMcpTool, ExtractTextContentFromDom>();
    }
}
