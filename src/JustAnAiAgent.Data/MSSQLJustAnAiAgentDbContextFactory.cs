using cCoder.Security.Objects;
using JustAnAiAgent.Data.Interfaces;

namespace JustAnAiAgent.Data;

public class MSSQLJustAnAiAgentDbContextFactory() : IJustAnAiAgentDbContextFactory
{
    private readonly string connectionString;

    public Func<bool, ISSOAuthInfo> GetAuthInfo { get; set; } =
        (x) => new SSOAuthInfo { SSOUserId = "Guest" };

    public MSSQLJustAnAiAgentDbContextFactory(string connectionString) : this() =>
        this.connectionString = connectionString;

    public JustAnAiAgentDbContext CreateDbContext(bool ignoreAuthInfo = false) =>
        new(GetAuthInfo(ignoreAuthInfo), new MSSQLJustAnAiAgentModelBuildProvider(connectionString ?? "AgentDb"));

    public JustAnAiAgentDbContext CreateDbContext(string[] args) =>
        CreateDbContext();
}