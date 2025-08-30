namespace JustAnAiAgent.MCP.MCP.GetDirectoryTree;

public class ItemNode
{
    public string Name { get; set; }
    
    public string Type { get; set; }

    public IEnumerable<ItemNode> Children { get; set; }
}
