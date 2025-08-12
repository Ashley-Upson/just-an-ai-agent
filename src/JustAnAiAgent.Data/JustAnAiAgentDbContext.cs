using cCoder.Security.Objects;
using JustAnAiAgent.Data.Entities;
using JustAnAiAgent.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JustAnAiAgent.Data;

public class JustAnAiAgentDbContext(
    ISSOAuthInfo authInfo,
    IJustAnAiAgentModelBuildProvider provider)
        : DbContext
{
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<UserConversation> UserConversations => Set<UserConversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<AgenticProject> AgenticProjects => Set<AgenticProject>();
    public DbSet<ProjectTask> ProjectTasks => Set<ProjectTask>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        provider.Create(modelBuilder);
    }

    public void Migrate() =>
        provider.MigrateDatabase(Database);
}