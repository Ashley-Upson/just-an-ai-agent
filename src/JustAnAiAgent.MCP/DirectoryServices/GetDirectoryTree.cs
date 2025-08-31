using System.ComponentModel.DataAnnotations;
using JustAnAiAgent.MCP.Interfaces;
using JustAnAiAgent.MCP.MCP;

namespace JustAnAiAgent.MCP.DirectoryServices;

public class GetDirectoryTree : IMcpTool
{
    public string Name => "get-directory-tree";

    public IEnumerable<ToolParameter> Parameters = new List<ToolParameter>()
    {
        new()
        {
            Name = "path",
            Description = "The root path for the tree being requested.",
            Type = "string",
            Required = true,
        }
    };

    public IEnumerable<string> FilteredFolders = new List<string>();

    public async ValueTask<string> Execute(IEnumerable<ToolParameterInput> parameters)
    {
        var pathParameter = parameters.Where(p => p.Name == "path").FirstOrDefault();

        if (pathParameter == null)
            throw new ValidationException("Parameter 'path' not specified.");

        var path = pathParameter.Value;

        IEnumerable<string> tree = GetDirectoryTreeFromPath(path);

        return string.Join("\n", tree);
    }

    public IEnumerable<ToolParameter> GetParameters() =>
        Parameters;

    public ToolDefinition GetToolDefinition()
    {
        return new()
        {
            Name = Name,
            Description = "List all files recursively in a given directory",
            Type = "function",
            Parameters = Parameters,
            Required = Parameters.Where(p => p.Required).Select(p => p.Name).ToArray()
        };
    }

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