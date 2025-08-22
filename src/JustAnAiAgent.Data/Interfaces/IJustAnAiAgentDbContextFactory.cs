using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace JustAnAiAgent.Data.Interfaces;

public interface IJustAnAiAgentDbContextFactory
{
    JustAnAiAgentDbContext CreateDbContext();
}