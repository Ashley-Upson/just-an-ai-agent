using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using JustAnAiAgent.MCP.Interfaces;
using JustAnAiAgent.MCP.MCP;

namespace JustAnAiAgent.MCP.Tools.PowerShell;

public partial class RunPowerShellCommand : IMcpTool
{
    private const int DefaultTimeoutSeconds = 60;
    private const int MaximumTimeoutSeconds = 300;
    private const int MaximumOutputLength = 100_000;

    public string Name => "run-powershell-command";

    private readonly ToolParameters parameters = new()
    {
        Type = "object",
        Properties = new List<ToolParameterProperty>
        {
            new()
            {
                Name = "command",
                Type = "string",
                Description = "The PowerShell command or script body to run. This is scanned for blocked escape-oriented patterns before execution.",
                Required = true,
            },
            new()
            {
                Name = "workingDirectory",
                Type = "string",
                Description = "Optional working directory relative to the project root. It is created if it does not exist.",
                Required = false,
            },
            new()
            {
                Name = "timeoutSeconds",
                Type = "number",
                Description = "Optional timeout in seconds. Maximum is 300 seconds.",
                Required = false,
            }
        },
        Required = new List<string> { "command" },
    };

    private static readonly string[] BlockedLiteralFragments =
    [
        "../",
        @"..\",
        "~/",
        @"~\",
        "$home",
        "$env:userprofile",
        "$env:homedrive",
        "$env:homepath"
    ];

    private static readonly Regex[] BlockedPatterns =
    [
        ChangeDirectoryRegex(),
        LocationStackRegex(),
        InvokeExpressionRegex(),
        StartProcessRegex(),
        PowerShellExecutableRegex(),
        PsDriveRegex(),
        ProviderPathRegex(),
        DriveRootPathRegex(),
        UncPathRegex(),
        DotDotPathRegex()
    ];

    public ToolParameters GetParameters() =>
        parameters;

    public ToolDefinition GetToolDefinition()
    {
        return new()
        {
            Name = Name,
            Description = "Run a non-interactive PowerShell command from a project-scoped working directory. This is a guardrail against accidental project escape, not a security sandbox.",
            Type = "function",
            Parameters = parameters,
            Required = parameters.Properties.Where(p => p.Required).Select(p => p.Name).ToArray()
        };
    }

    public async ValueTask<string> Execute(IEnumerable<ToolParameterInput> parameters)
    {
        return await Execute(parameters, new ToolExecutionContext());
    }

    public async ValueTask<string> Execute(IEnumerable<ToolParameterInput> parameters, ToolExecutionContext context)
    {
        Dictionary<string, object> parameterValues = parameters.ToDictionary(
            parameter => parameter.Name,
            parameter => parameter.Value,
            StringComparer.OrdinalIgnoreCase);

        string projectPath = context.ProjectPath
            ?? throw new ValidationException("PowerShell execution requires an orchestration-provided project path.");
        string command = GetRequiredString(parameterValues, "command");
        string workingDirectoryParameter = GetOptionalString(parameterValues, "workingDirectory") ?? ".";
        int timeoutSeconds = GetTimeoutSeconds(parameterValues);

        ValidateCommand(command);

        string normalizedProjectPath = Path.GetFullPath(projectPath);
        Directory.CreateDirectory(normalizedProjectPath);

        string workingDirectory = ResolveWorkingDirectory(normalizedProjectPath, workingDirectoryParameter);
        Directory.CreateDirectory(workingDirectory);

        string scriptPath = Path.Combine(workingDirectory, $".just-an-ai-agent-{Guid.NewGuid():N}.ps1");

        try
        {
            await File.WriteAllTextAsync(scriptPath, command, Encoding.UTF8);

            PowerShellCommandResult result = await RunScriptAsync(
                workingDirectory,
                scriptPath,
                timeoutSeconds);

            return JsonSerializer.Serialize(result);
        }
        finally
        {
            if (File.Exists(scriptPath))
                File.Delete(scriptPath);
        }
    }

    private static async ValueTask<PowerShellCommandResult> RunScriptAsync(
        string workingDirectory,
        string scriptPath,
        int timeoutSeconds)
    {
        using Process process = new()
        {
            StartInfo = new()
            {
                FileName = GetPowerShellExecutable(),
                Arguments = $"-NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File {QuoteArgument(scriptPath)}",
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        StringBuilder standardOutput = new();
        StringBuilder standardError = new();

        process.OutputDataReceived += (_, eventArgs) => AppendOutput(standardOutput, eventArgs.Data);
        process.ErrorDataReceived += (_, eventArgs) => AppendOutput(standardError, eventArgs.Data);

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        using CancellationTokenSource cancellationTokenSource = new(TimeSpan.FromSeconds(timeoutSeconds));

        bool timedOut = false;

        try
        {
            await process.WaitForExitAsync(cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            timedOut = true;

            if (!process.HasExited)
                process.Kill(entireProcessTree: true);

            await process.WaitForExitAsync();
        }

        return new()
        {
            ExitCode = timedOut ? null : process.ExitCode,
            TimedOut = timedOut,
            StandardOutput = standardOutput.ToString(),
            StandardError = standardError.ToString()
        };
    }

    private static string ResolveWorkingDirectory(string projectPath, string workingDirectoryParameter)
    {
        string workingDirectory = Path.IsPathRooted(workingDirectoryParameter)
            ? Path.GetFullPath(workingDirectoryParameter)
            : Path.GetFullPath(Path.Combine(projectPath, workingDirectoryParameter));

        if (!IsInsideRoot(projectPath, workingDirectory))
            throw new ValidationException("The requested working directory is outside the project path.");

        return workingDirectory;
    }

    private static bool IsInsideRoot(string root, string path)
    {
        string normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string normalizedPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        return string.Equals(normalizedRoot, normalizedPath, StringComparison.OrdinalIgnoreCase)
            || normalizedPath.StartsWith(normalizedRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            || normalizedPath.StartsWith(normalizedRoot + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    private static void ValidateCommand(string command)
    {
        string normalizedCommand = command.ToLowerInvariant();

        foreach (string fragment in BlockedLiteralFragments)
        {
            if (normalizedCommand.Contains(fragment))
                throw new ValidationException($"PowerShell command contains a blocked escape-oriented fragment: {fragment}");
        }

        foreach (Regex pattern in BlockedPatterns)
        {
            if (pattern.IsMatch(command))
                throw new ValidationException($"PowerShell command contains a blocked pattern: {pattern}");
        }
    }

    private static string GetRequiredString(Dictionary<string, object> values, string name)
    {
        string? value = GetOptionalString(values, name);

        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException($"Parameter '{name}' is required.");

        return value;
    }

    private static string? GetOptionalString(Dictionary<string, object> values, string name) =>
        values.TryGetValue(name, out object? value)
            ? value?.ToString()
            : null;

    private static int GetTimeoutSeconds(Dictionary<string, object> values)
    {
        string? value = GetOptionalString(values, "timeoutSeconds");

        if (!int.TryParse(value, out int timeoutSeconds))
            return DefaultTimeoutSeconds;

        return Math.Clamp(timeoutSeconds, 1, MaximumTimeoutSeconds);
    }

    private static string GetPowerShellExecutable()
    {
        string? pwshPath = FindOnPath(OperatingSystem.IsWindows() ? "pwsh.exe" : "pwsh");

        if (!string.IsNullOrWhiteSpace(pwshPath))
            return pwshPath;

        return OperatingSystem.IsWindows()
            ? "powershell.exe"
            : "pwsh";
    }

    private static string? FindOnPath(string executableName)
    {
        string? path = Environment.GetEnvironmentVariable("PATH");

        if (string.IsNullOrWhiteSpace(path))
            return null;

        foreach (string directory in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            string candidate = Path.Combine(directory, executableName);

            if (File.Exists(candidate))
                return candidate;
        }

        return null;
    }

    private static string QuoteArgument(string argument) =>
        $"\"{argument.Replace("\"", "\\\"")}\"";

    private static void AppendOutput(StringBuilder output, string? text)
    {
        if (text is null || output.Length >= MaximumOutputLength)
            return;

        int remainingLength = MaximumOutputLength - output.Length;
        string value = text.Length > remainingLength
            ? text[..remainingLength]
            : text;

        output.AppendLine(value);
    }

    [GeneratedRegex(@"(?<![\w-])(cd|chdir|set-location|sl)(?![\w-])", RegexOptions.IgnoreCase)]
    private static partial Regex ChangeDirectoryRegex();

    [GeneratedRegex(@"(?<![\w-])(push-location|pop-location|pushd|popd)(?![\w-])", RegexOptions.IgnoreCase)]
    private static partial Regex LocationStackRegex();

    [GeneratedRegex(@"(?<![\w-])(invoke-expression|iex)(?![\w-])", RegexOptions.IgnoreCase)]
    private static partial Regex InvokeExpressionRegex();

    [GeneratedRegex(@"(?<![\w-])(start-process|saps|start)(?![\w-])", RegexOptions.IgnoreCase)]
    private static partial Regex StartProcessRegex();

    [GeneratedRegex(@"(?<![\w-])(powershell|powershell\.exe|pwsh|pwsh\.exe)(?![\w-])", RegexOptions.IgnoreCase)]
    private static partial Regex PowerShellExecutableRegex();

    [GeneratedRegex(@"(?<![\w-])(new-psdrive|remove-psdrive|get-psdrive)(?![\w-])", RegexOptions.IgnoreCase)]
    private static partial Regex PsDriveRegex();

    [GeneratedRegex(@"\b[A-Za-z][A-Za-z0-9+.-]*::", RegexOptions.IgnoreCase)]
    private static partial Regex ProviderPathRegex();

    [GeneratedRegex(@"(?<![\w])([A-Za-z]:[\\/])", RegexOptions.IgnoreCase)]
    private static partial Regex DriveRootPathRegex();

    [GeneratedRegex(@"(^|[^\w])\\\\[^\\/\s]+[\\/][^\\/\s]+", RegexOptions.IgnoreCase)]
    private static partial Regex UncPathRegex();

    [GeneratedRegex(@"(^|[\\/])\.\.([\\/]|$)", RegexOptions.IgnoreCase)]
    private static partial Regex DotDotPathRegex();
}
