namespace JustAnAiAgent.MCP.Tools.PowerShell;

public class PowerShellCommandResult
{
    public int? ExitCode { get; set; }

    public bool TimedOut { get; set; }

    public string StandardOutput { get; set; } = string.Empty;

    public string StandardError { get; set; } = string.Empty;
}
