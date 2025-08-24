using JustAnAiAgent.Objects.Entities;

namespace JustAnAiAgent.Services.Processing.Interfaces;

public interface IMessageProcessingService
{
    IQueryable<Message> GetAll();

    ValueTask<Message> GetAsync(Guid id);

    ValueTask<Message> AddAsync(Message message);

    ValueTask<Message> UpdateAsync(Guid id, Message message);

    Task DeleteAsync(Guid id);
}
