using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using JustAnAiAgent.MCP.Interfaces;
using JustAnAiAgent.MCP.MCP;

namespace JustAnAiAgent.MCP.MCP.Files;

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

    protected static async ValueTask<string> ExecuteFileOperation(Func<ValueTask<FileHandlerResult>> operation)
    {
        try
        {
            return SerializeResult(await operation());
        }
        catch (ValidationException exception)
        {
            return SerializeFailure(exception.Message);
        }
        catch (UnauthorizedAccessException exception)
        {
            return SerializeFailure($"File operation failed because access was denied: {exception.Message}");
        }
        catch (IOException exception)
        {
            return SerializeFailure($"File operation failed because of an IO error: {exception.Message}");
        }
        catch (HttpRequestException exception)
        {
            return SerializeFailure($"File download failed: {exception.Message}");
        }
        catch (InvalidDataException exception)
        {
            return SerializeFailure($"Archive operation failed: {exception.Message}");
        }
        catch (Exception exception)
        {
            return SerializeFailure($"File operation failed: {exception.Message}");
        }
    }

    protected static string SerializeFailure(string message) =>
        SerializeResult(new()
        {
            Success = false,
            Message = message
        });

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

    protected static int? GetOptionalInteger(Dictionary<string, object> parameters, string name)
    {
        string? value = GetOptionalString(parameters, name);

        return int.TryParse(value, out int result)
            ? result
            : null;
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
