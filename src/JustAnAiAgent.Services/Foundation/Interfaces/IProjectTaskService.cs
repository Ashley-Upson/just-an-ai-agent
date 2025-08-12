using JustAnAiAgent.Data.Entities;

namespace JustAnAiAgent.Services.Foundation.Interfaces;

interface IProjectTaskService
{
    IQueryable<ProjectTask> GetAll();

    ValueTask<ProjectTask> Get(Guid id);

    ValueTask<ProjectTask> Add(ProjectTask task);

    ValueTask<ProjectTask> Update(ProjectTask task);

    void Delete(ProjectTask task);
}