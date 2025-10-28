using System.ComponentModel.DataAnnotations;
using JustAnAiAgent.MCP.Interfaces;
using JustAnAiAgent.MCP.MCP;

namespace JustAnAiAgent.MCP.Tools.DirectoryServices;

public class GetDirectoryTree : IMcpTool
{
    public string Name => "get-directory-tree";

    private ToolParameters Parameters = new()
    {
        Type = "object",
        Properties = new List<ToolParameterProperty> {
            new() {
                Name = "path",
                Type = "string",
                Description = "The root path for the tree being requested.",
                Required = true,
            }
        },
        Required = new List<string> { "path" },
    };

    public ToolDefinition GetToolDefinition()
    {
        return new()
        {
            Name = Name,
            Description = "List all files recursively in a given directory",
            Type = "function",
            Parameters = Parameters,
            Required = Parameters.Properties.Where(p => p.Required).Select(p => p.Name).ToArray()
        };
    }

    public List<string> FilteredFolders = new();

    public async ValueTask<string> Execute(IEnumerable<ToolParameterInput> parameters)
    {
        var pathParameter = parameters.Where(p => p.Name == "path").FirstOrDefault();
        var filterParameter = parameters.Where(p => p.Name == "filter").FirstOrDefault();

        if (pathParameter == null)
            throw new ValidationException("Parameter 'path' not specified.");

        if(filterParameter is null)
            FilteredFolders.AddRange([
                ".vs",
                ".git",
                "bin",
                "obj"
            ]);
        else
            if (filterParameter.Value is IEnumerable<object> values)
                FilteredFolders.AddRange(values.Select(i => i?.ToString() ?? string.Empty).Where(i => i != null));
            else
                throw new ValidationException("Filter parameter value is not in the expected format.");

        var path = pathParameter.Value.ToString();

        IEnumerable<string> tree = GetDirectoryTreeFromPath(path);

        return string.Join("\n", tree);
    }

    public ToolParameters GetParameters() =>
        Parameters;

    private string[] GetDirectoryTreeFromPath(string path)
    {
        DirectoryInfo directory = new DirectoryInfo(path);

        string parentPath = directory.Parent.FullName;

        string[] tree = GetFilesAndFolders(path);

        return tree.Select(i => i.Replace(parentPath, "")).ToArray();
    }

    private string[] GetFilesAndFolders(string path)
    {
        DirectoryInfo directory = new DirectoryInfo(path);
        FileSystemInfo[] items = directory.GetFileSystemInfos();
        List<string> tree = new();

        foreach (FileSystemInfo item in items)
        {
            if (item is DirectoryInfo && FilteredFolders.Contains(item.Name))
                continue;

            if(item is FileInfo)
                tree.Add(item.FullName);

            if (item is DirectoryInfo)
                tree.AddRange(GetFilesAndFolders(item.FullName));
        }

        return tree.ToArray();
    }
}