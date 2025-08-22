using JustAnAiAgent.Data.Brokers.Interfaces;
using JustAnAiAgent.Objects.Entities;
using JustAnAiAgent.Services.Foundation.Interfaces;

namespace JustAnAiAgent.Services.Foundation;

public class AgenticProjectService(IAgenticProjectBroker broker) : IAgenticProjectService
{
    public IQueryable<AgenticProject> GetAll() =>
        broker.GetAll();

    public async ValueTask<AgenticProject> GetAsync(Guid id) =>
        await broker.GetAsync(id);

    public async ValueTask<AgenticProject> AddAsync(AgenticProject project)
    {
        project.CreatedAt = DateTimeOffset.UtcNow;

        return await broker.AddAsync(project);
    }

    public async ValueTask<AgenticProject> UpdateAsync(Guid id, AgenticProject project)
    {
        project.UpdatedAt = DateTimeOffset.UtcNow;

        return await broker.UpdateAsync(id, project);
    }

    public async Task DeleteAsync(Guid id) =>
        await broker.DeleteAsync(id);
}
