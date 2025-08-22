using JustAnAiAgent.Objects.Entities;

namespace JustAnAiAgent.Data.Brokers.Interfaces;

public interface IMessageBroker
{
    IQueryable<Message> GetAll();

    ValueTask<Message> GetAsync(Guid id);

    ValueTask<Message> AddAsync(Message message);

    ValueTask<Message> UpdateAsync(Guid id, Message message);

    Task DeleteAsync(Guid id);
}
