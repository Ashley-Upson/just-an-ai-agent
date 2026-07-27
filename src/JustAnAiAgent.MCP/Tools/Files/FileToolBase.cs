using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using JustAnAiAgent.MCP.Interfaces;
using JustAnAiAgent.MCP.MCP;

namespace JustAnAiAgent.MCP.Tools.Files;

public abstract class FileToolBase(FileHandler fileHandler) : IMcpTool
{
    protected FileHandler FileHandler { get; } = fileHandler;

    public abstract string Name { get; }

    protected abstract string Description { get; }

    protected abstract ToolParameters Parameters { get; }

    public ToolParameters GetParameters() =>
        Parameters;

    public ToolDefinition GetToolDefinition()
    {
        return new()
        {
            Name = Name,
            Description = Description,
            Type = "function",
            Parameters = Parameters,
            Required = Parameters.Properties.Where(p => p.Required).Select(p => p.Name).ToArray()
        };
    }

    public async ValueTask<string> Execute(IEnumerable<ToolParameterInput> parameters)
    {
        return await Execute(parameters, new ToolExecutionContext());
    }

    public abstract ValueTask<string> Execute(IEnumerable<ToolParameterInput> parameters, ToolExecutionContext context);

    protected static string SerializeResult(FileHandlerResult result) =>
        JsonSerializer.Serialize(result);

    protected static Dictionary<string, object> ToParameterDictionary(IEnumerable<ToolParameterInput> parameters) =>
        parameters.ToDictionary(
            parameter => parameter.Name,
            parameter => parameter.Value,
            StringComparer.OrdinalIgnoreCase);

    protected static string GetRequiredString(Dictionary<string, object> parameters, string name)
    {
        string? value = GetOptionalString(parameters, name);

        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException($"Parameter '{name}' is required.");

        return value;
    }

    protected static string? GetOptionalString(Dictionary<string, object> parameters, string name) =>
        parameters.TryGetValue(name, out object? value)
            ? value?.ToString()
            : null;

    protected static bool GetOptionalBoolean(Dictionary<string, object> parameters, string name, bool defaultValue = false)
    {
        string? value = GetOptionalString(parameters, name);

        return bool.TryParse(value, out bool result)
            ? result
            : defaultValue;
    }

    protected static ToolParameterProperty Parameter(string name, string type, string description, bool required) =>
        new()
        {
            Name = name,
            Type = type,
            Description = description,
            Required = required
        };
}
