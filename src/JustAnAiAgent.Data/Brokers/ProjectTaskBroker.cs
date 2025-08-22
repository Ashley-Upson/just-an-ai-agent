using System.Security.Authentication;
using cCoder.Security.Objects;
using JustAnAiAgent.Data.Brokers.Interfaces;
using JustAnAiAgent.Objects.Entities;
using JustAnAiAgent.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace JustAnAiAgent.Data.Brokers;

public class ProjectTaskBroker(
    IJustAnAiAgentDbContextFactory contextFactory,
    ISSOAuthInfo authInfo) : IProjectTaskBroker
{
    public IQueryable<ProjectTask> GetAll()
    {
        var context = contextFactory.CreateDbContext();

        IQueryable<ProjectTask> result = context.ProjectTasks
            .Where(pt => pt.AgenticProject.Message.Conversation.Users.Any(uc => uc.UserId == authInfo.SSOUserId));

        return result;
    }

    public async ValueTask<ProjectTask> GetAsync(Guid id)
    {
        using var context = contextFactory.CreateDbContext();

        return await context.ProjectTasks
            .Where(pt => pt.AgenticProject.Message.Conversation.Users.Any(cu => cu.UserId == authInfo.SSOUserId))
            .FirstOrDefaultAsync(pt => pt.Id == id);
    }

    public async ValueTask<ProjectTask> AddAsync(ProjectTask task)
    {
        using var context = contextFactory.CreateDbContext();

        var project = await context.AgenticProjects
            .Where(ap => ap.Message.Conversation.Users.Any(cu => cu.UserId == authInfo.SSOUserId))
            .FirstOrDefaultAsync(ap => ap.Id == task.ProjectId);

        if (project is null)
            throw new AuthenticationException("Access denied.");

        EntityEntry<ProjectTask> entry = await context.ProjectTasks.AddAsync(task);

        await context.SaveChangesAsync();

        return entry.Entity;
    }

    public async ValueTask<ProjectTask> UpdateAsync(Guid id, ProjectTask task)
    {
        using var context = contextFactory.CreateDbContext();

        var project = await GetAsync(id);

        if (project is null)
            throw new AuthenticationException("Access denied.");

        EntityEntry<ProjectTask> entry = context.ProjectTasks.Update(task);

        await context.SaveChangesAsync();

        return entry.Entity;
    }

    public async Task DeleteAsync(Guid id)
    {
        using var context = contextFactory.CreateDbContext();

        var project = await GetAsync(id);

        if (project is null)
            throw new AuthenticationException("Access denied.");

        context.ProjectTasks.Remove(new ProjectTask { Id = id });

        await context.SaveChangesAsync();
    }
}