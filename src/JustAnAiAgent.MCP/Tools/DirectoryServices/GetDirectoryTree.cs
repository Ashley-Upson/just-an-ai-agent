using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using JustAnAiAgent.MCP.Interfaces;
using JustAnAiAgent.MCP.MCP;
using JustAnAiAgent.MCP.MCP.Files;

namespace JustAnAiAgent.MCP.Tools.DirectoryServices;

public class GetDirectoryTree(FileHandler fileHandler) : IMcpTool
{
    public string Name => "get-directory-tree";

    private readonly ToolParameters parameters = new()
    {
        Type = "object",
        Properties = new List<ToolParameterProperty>
        {
            new()
            {
                Name = "path",
                Type = "string",
                Description = "Relative root path for the tree being requested.",
                Required = true,
            },
            new()
            {
                Name = "filter",
                Type = "array",
                Description = "Optional folder names to exclude. Defaults to .vs, .git, bin, and obj.",
                Required = false,
            }
        },
        Required = new List<string> { "path" },
    };

    public ToolDefinition GetToolDefinition()
    {
        return new()
        {
            Name = Name,
            Description = "List all files recursively in a directory under the current tool workspace.",
            Type = "function",
            Parameters = parameters,
            Required = parameters.Properties.Where(p => p.Required).Select(p => p.Name).ToArray()
        };
    }

    public ToolParameters GetParameters() =>
        parameters;

    public async ValueTask<string> Execute(IEnumerable<ToolParameterInput> parameters)
    {
        return await Execute(parameters, new ToolExecutionContext());
    }

    public async ValueTask<string> Execute(IEnumerable<ToolParameterInput> parameters, ToolExecutionContext context)
    {
        try
        {
            Dictionary<string, object> values = parameters.ToDictionary(
                parameter => parameter.Name,
                parameter => parameter.Value,
                StringComparer.OrdinalIgnoreCase);
            FileHandlerResult result = await fileHandler.GetDirectoryTreeAsync(
                context,
                GetRequiredString(values, "path"),
                GetOptionalFilter(values));

            return JsonSerializer.Serialize(result.Paths ?? []);
        }
        catch (ValidationException exception)
        {
            return SerializeFailure(exception.Message);
        }
        catch (UnauthorizedAccessException exception)
        {
            return SerializeFailure($"Directory tree read failed because access was denied: {exception.Message}");
        }
        catch (IOException exception)
        {
            return SerializeFailure($"Directory tree read failed because of an IO error: {exception.Message}");
        }
        catch (Exception exception)
        {
            return SerializeFailure($"Directory tree read failed: {exception.Message}");
        }
    }

    private static string SerializeFailure(string message) =>
        JsonSerializer.Serialize(new FileHandlerResult
        {
            Success = false,
            Message = message
        });

    private static string GetRequiredString(Dictionary<string, object> values, string name)
    {
        string? value = values.TryGetValue(name, out object? rawValue)
            ? rawValue?.ToString()
            : null;

        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException($"Parameter '{name}' is required.");

        return value;
    }

    private static IEnumerable<string>? GetOptionalFilter(Dictionary<string, object> values)
    {
        if (!values.TryGetValue("filter", out object? filter))
            return null;

        if (filter is IEnumerable<object> filterValues)
            return filterValues
                .Select(value => value?.ToString())
                .Where(value => !string.IsNullOrWhiteSpace(value))!;

        return filter.ToString()?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
