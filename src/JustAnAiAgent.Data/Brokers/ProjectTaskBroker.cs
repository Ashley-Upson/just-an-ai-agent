using JustAnAiAgent.Data.Brokers.Interfaces;
using JustAnAiAgent.Data.Entities;
using JustAnAiAgent.Data.Interfaces;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace JustAnAiAgent.Data.Brokers;

public class ProjectTaskBroker(IJustAnAiAgentDbContextFactory contextFactory) : IProjectTaskBroker
{
    public IQueryable<ProjectTask> GetAll()
    {
        using var context = contextFactory.CreateDbContext();

        IQueryable<ProjectTask> result = context.ProjectTasks;

        return result;
    }

    public async ValueTask<ProjectTask> Get(Guid id)
    {
        using var context = contextFactory.CreateDbContext();

        return await context.ProjectTasks.FindAsync(id);
    }

    public async ValueTask<ProjectTask> Add(ProjectTask task)
    {
        using var context = contextFactory.CreateDbContext();

        EntityEntry<ProjectTask> entry = await context.ProjectTasks.AddAsync(task);

        await context.SaveChangesAsync();

        return entry.Entity;
    }

    public async ValueTask<ProjectTask> Update(ProjectTask task)
    {
        using var context = contextFactory.CreateDbContext();

        EntityEntry<ProjectTask> entry = context.ProjectTasks.Update(task);

        await context.SaveChangesAsync();

        return entry.Entity;
    }

    public async void Delete(ProjectTask task)
    {
        using var context = contextFactory.CreateDbContext();

        context.ProjectTasks.Remove(new ProjectTask { Id = task.Id });

        await context.SaveChangesAsync();
    }
}