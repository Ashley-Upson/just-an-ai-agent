using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using JustAnAiAgent.MCP.MCP;
using JustAnAiAgent.MCP.MCP.GetDirectoryTree;

namespace JustAnAiAgent.MCP.DirectoryServices;

public class GetDirectoryTree
{
    public string Name = "get-directory-tree";

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

    public string Execute(IEnumerable<ToolParameterInput> parameters)
    {
        var pathParameter = parameters.Where(p => p.Name == "path").FirstOrDefault();

        if (pathParameter == null)
            throw new ValidationException("Parameter 'path' not specified.");

        var path = pathParameter.Value;

        IEnumerable<ItemNode> tree = GetDirectoryTreeFromPath(path);

        return JsonSerializer.Serialize(tree);
    }

    private IEnumerable<ItemNode> GetDirectoryTreeFromPath(string path)
    {
        var tree = new List<ItemNode>();

        DirectoryInfo directory = new DirectoryInfo(path);
        FileSystemInfo[] items = directory.GetFileSystemInfos();

        foreach (FileSystemInfo item in items)
        {
            if (item is DirectoryInfo && FilteredFolders.Contains(item.Name))
                continue;

            if (item is DirectoryInfo)
            {
                tree.Add(new()
                {
                    Name = item.Name,
                    Type = "directory",
                    Children = GetDirectoryTreeFromPath(item.FullName)
                });
            }

            if (item is FileInfo)
            {
                tree.Add(new()
                {
                    Name = item.Name,
                    Type = "file",
                });
            }
        }

        return tree;
    }
}