using JustAnAiAgent.Data.Entities;

namespace JustAnAiAgent.Data.Brokers.Interfaces;

interface IAgenticProjectBroker
{
    IQueryable<AgenticProject> GetAll();

    ValueTask<AgenticProject> Get(Guid id);

    ValueTask<AgenticProject> Add(AgenticProject project);

    ValueTask<AgenticProject> Update(AgenticProject project);

    void Delete(AgenticProject project);
}