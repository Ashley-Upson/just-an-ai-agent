using Microsoft.EntityFrameworkCore.Design;

namespace JustAnAiAgent.Data.Interfaces;

public interface IJustAnAiAgentDbContextFactory : IDesignTimeDbContextFactory<JustAnAiAgentDbContext>
{
    JustAnAiAgentDbContext CreateDbContext(bool ignoreAuthInfo = false);
}