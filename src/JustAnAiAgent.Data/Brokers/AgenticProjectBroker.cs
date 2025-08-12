using JustAnAiAgent.Data.Brokers.Interfaces;
using JustAnAiAgent.Data.Entities;
using JustAnAiAgent.Data.Interfaces;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace JustAnAiAgent.Data.Brokers;

public class AgenticProjectBroker(IJustAnAiAgentDbContextFactory contextFactory) : IAgenticProjectBroker
{
    public IQueryable<AgenticProject> GetAll()
    {
        using var context = contextFactory.CreateDbContext();

        IQueryable<AgenticProject> result = context.AgenticProjects;

        return result;
    }

    public async ValueTask<AgenticProject> Get(Guid id)
    {
        using var context = contextFactory.CreateDbContext();

        return await context.AgenticProjects.FindAsync(id);
    }

    public async ValueTask<AgenticProject> Add(AgenticProject project)
    {
        using var context = contextFactory.CreateDbContext();

        EntityEntry<AgenticProject> entry = await context.AgenticProjects.AddAsync(project);

        await context.SaveChangesAsync();

        return entry.Entity;
    }

    public async ValueTask<AgenticProject> Update(AgenticProject project)
    {
        using var context = contextFactory.CreateDbContext();

        EntityEntry<AgenticProject> entry = context.AgenticProjects.Update(project);

        await context.SaveChangesAsync();

        return entry.Entity;
    }

    public async void Delete(AgenticProject project)
    {
        using var context = contextFactory.CreateDbContext();

        context.AgenticProjects.Remove(new AgenticProject { Id = project.Id });

        await context.SaveChangesAsync();
    }
}