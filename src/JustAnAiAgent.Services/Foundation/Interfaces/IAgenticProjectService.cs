using JustAnAiAgent.Objects.Entities;

namespace JustAnAiAgent.Services.Foundation.Interfaces;

public interface IAgenticProjectService
{
    IQueryable<AgenticProject> GetAll();

    ValueTask<AgenticProject> GetAsync(Guid id);

    ValueTask<AgenticProject> AddAsync(AgenticProject project);

    ValueTask<AgenticProject> UpdateAsync(Guid id, AgenticProject project);

    Task DeleteAsync(Guid id);
}