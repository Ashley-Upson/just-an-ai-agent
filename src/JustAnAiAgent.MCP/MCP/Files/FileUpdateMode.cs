using System.ComponentModel.DataAnnotations;

namespace JustAnAiAgent.MCP.MCP.Files;

public enum FileUpdateMode
{
    Overwrite,
    Prepend,
    Append,
    Insert,
    ReplaceLines
}

public static class FileUpdateModeParser
{
    public static FileUpdateMode Parse(string? mode)
    {
        if (string.IsNullOrWhiteSpace(mode))
            return FileUpdateMode.Overwrite;

        return mode.Trim().ToLowerInvariant() switch
        {
            "overwrite" => FileUpdateMode.Overwrite,
            "prepend" => FileUpdateMode.Prepend,
            "append" => FileUpdateMode.Append,
            "insert" => FileUpdateMode.Insert,
            "replacelines" => FileUpdateMode.ReplaceLines,
            "replace-lines" => FileUpdateMode.ReplaceLines,
            "replace_lines" => FileUpdateMode.ReplaceLines,
            _ => throw new ValidationException("Parameter 'mode' must be one of: overwrite, prepend, append, insert, replaceLines.")
        };
    }
}
