using JustAnAiAgent.MCP.Interfaces;
using JustAnAiAgent.MCP.MCP.Files;
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
        services.AddTransient<IMcpTool, CreateFile>();
        services.AddTransient<IMcpTool, ReadFile>();
        services.AddTransient<IMcpTool, UpdateFile>();
        services.AddTransient<IMcpTool, DeleteFile>();
        services.AddTransient<IMcpTool, RenameFile>();
        services.AddTransient<IMcpTool, MoveFile>();
        services.AddTransient<IMcpTool, ZipFile>();
        services.AddTransient<IMcpTool, UnzipFile>();
        services.AddTransient<IMcpTool, DuckDuckGoSearch>();
        services.AddTransient<IMcpTool, GetWebContentFromUrl>();
        services.AddTransient<IMcpTool, ExtractTextContentFromDom>();
    }
}
