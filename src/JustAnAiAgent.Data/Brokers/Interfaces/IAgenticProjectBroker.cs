using JustAnAiAgent.Objects.Entities;

namespace JustAnAiAgent.Data.Brokers.Interfaces;

public interface IAgenticProjectBroker
{
    IQueryable<AgenticProject> GetAll();

    ValueTask<AgenticProject> GetAsync(Guid id);

    ValueTask<AgenticProject> AddAsync(AgenticProject project);

    ValueTask<AgenticProject> UpdateAsync(Guid id, AgenticProject project);

    Task DeleteAsync(Guid id);
}