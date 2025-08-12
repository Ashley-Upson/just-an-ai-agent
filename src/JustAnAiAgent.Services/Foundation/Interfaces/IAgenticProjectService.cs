using JustAnAiAgent.Data.Entities;

namespace JustAnAiAgent.Services.Foundation.Interfaces;

interface IAgenticProjectService
{
    IQueryable<AgenticProject> GetAll();

    ValueTask<AgenticProject> Get(Guid id);

    ValueTask<AgenticProject> Add(AgenticProject project);

    ValueTask<AgenticProject> Update(AgenticProject project);

    void Delete(AgenticProject project);
}