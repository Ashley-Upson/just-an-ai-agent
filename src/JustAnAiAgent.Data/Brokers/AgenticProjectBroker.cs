using System.Security.Authentication;
using cCoder.Security.Objects;
using JustAnAiAgent.Data.Brokers.Interfaces;
using JustAnAiAgent.Objects.Entities;
using JustAnAiAgent.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace JustAnAiAgent.Data.Brokers;

public class AgenticProjectBroker(
    IJustAnAiAgentDbContextFactory contextFactory,
    ISSOAuthInfo authInfo) : IAgenticProjectBroker
{
    public IQueryable<AgenticProject> GetAll()
    {
        var context = contextFactory.CreateDbContext();

        IQueryable<AgenticProject> result = context.AgenticProjects
            .Where(ap => ap.Conversations.Any(c => c.Users.Any(cu => cu.UserId == authInfo.SSOUserId)));

        return result;
    }

    public async ValueTask<AgenticProject> GetAsync(Guid id)
    {
        using var context = contextFactory.CreateDbContext();

        return await context.AgenticProjects
            .Where(ap => ap.Message.Conversation.Users.Any(cu => cu.UserId == authInfo.SSOUserId))
            .FirstOrDefaultAsync(ap => ap.Id == id);
    }

    public async ValueTask<AgenticProject> AddAsync(AgenticProject project)
    {
        using var context = contextFactory.CreateDbContext();

        AgenticProject dbProject = await context.AgenticProjects
            .Where(ap => ap.Message.Conversation.Users.Any(cu => cu.UserId == authInfo.SSOUserId))
            .FirstOrDefaultAsync(ap => ap.Id == project.Id);

        if (dbProject is null)
            throw new AuthenticationException("Access denied.");

        EntityEntry<AgenticProject> entry = await context.AgenticProjects.AddAsync(project);

        await context.SaveChangesAsync();

        return entry.Entity;
    }

    public async ValueTask<AgenticProject> UpdateAsync(Guid id, AgenticProject project)
    {
        using var context = contextFactory.CreateDbContext();

        AgenticProject dbProject = await GetAsync(id);

        if (dbProject is null)
            throw new AuthenticationException("Access denied.");

        EntityEntry<AgenticProject> entry = context.AgenticProjects.Update(project);

        await context.SaveChangesAsync();

        return entry.Entity;
    }

    public async Task DeleteAsync(Guid id)
    {
        using var context = contextFactory.CreateDbContext();

        AgenticProject project = await GetAsync(id);

        if (project is null)
            throw new AuthenticationException("Access denied.");

        context.AgenticProjects.Remove(new AgenticProject { Id = id });

        await context.SaveChangesAsync();
    }
}