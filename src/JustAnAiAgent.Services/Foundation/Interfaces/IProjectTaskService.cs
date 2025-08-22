using JustAnAiAgent.Objects.Entities;

namespace JustAnAiAgent.Services.Foundation.Interfaces;

public interface IProjectTaskService
{
    IQueryable<ProjectTask> GetAll();

    ValueTask<ProjectTask> GetAsync(Guid id);

    ValueTask<ProjectTask> AddAsync(ProjectTask task);

    ValueTask<ProjectTask> UpdateAsync(Guid id, ProjectTask task);

    Task DeleteAsync(Guid id);
}