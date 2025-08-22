using JustAnAiAgent.Objects.Entities;

namespace JustAnAiAgent.Data.Brokers.Interfaces;

public interface IProjectTaskBroker
{
    IQueryable<ProjectTask> GetAll();

    ValueTask<ProjectTask> GetAsync(Guid id);

    ValueTask<ProjectTask> AddAsync(ProjectTask task);

    ValueTask<ProjectTask> UpdateAsync(Guid id, ProjectTask task);

    Task DeleteAsync(Guid id);
}