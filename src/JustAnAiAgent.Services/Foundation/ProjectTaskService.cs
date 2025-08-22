using JustAnAiAgent.Data.Brokers.Interfaces;
using JustAnAiAgent.Objects.Entities;
using JustAnAiAgent.Services.Foundation.Interfaces;

namespace JustAnAiAgent.Services.Foundation;

public class ProjectTaskService(IProjectTaskBroker broker) : IProjectTaskService
{
    public IQueryable<ProjectTask> GetAll() =>
        broker.GetAll();

    public async ValueTask<ProjectTask> GetAsync(Guid id) =>
        await broker.GetAsync(id);

    public async ValueTask<ProjectTask> AddAsync(ProjectTask task)
    {
        task.CreatedAt = DateTimeOffset.UtcNow;

        return await broker.AddAsync(task);
    }

    public async ValueTask<ProjectTask> UpdateAsync(Guid id, ProjectTask task)
    {
        task.UpdatedAt = DateTimeOffset.UtcNow;

        return await broker.UpdateAsync(id, task);
    }

    public async Task DeleteAsync(Guid id) =>
        await broker.DeleteAsync(id);
}