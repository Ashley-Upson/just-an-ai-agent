using cCoder.Security.Objects;
using JustAnAiAgent.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JustAnAiAgent.Data;

public class MSSQLJustAnAiAgentDbContextFactory : IJustAnAiAgentDbContextFactory
{
    public IServiceProvider serviceProvider { get; }

    public MSSQLJustAnAiAgentDbContextFactory(IServiceProvider serviceProvider) =>
        this.serviceProvider = serviceProvider;

    public JustAnAiAgentDbContext CreateDbContext(DbContextOptions<JustAnAiAgentDbContext> options) =>
        CreateDbContext();

    public JustAnAiAgentDbContext CreateDbContext(string[] args) =>
        CreateDbContext();

    public JustAnAiAgentDbContext CreateDbContext() {
        //JustAnAiAgentDbContext context = serviceProvider.GetService<JustAnAiAgentDbContext>();

        JustAnAiAgentDbContext context = serviceProvider.CreateScope().ServiceProvider.GetService<JustAnAiAgentDbContext>();

        context.AuthInfo = serviceProvider.GetService<ISSOAuthInfo>();

        return context;
    }
}