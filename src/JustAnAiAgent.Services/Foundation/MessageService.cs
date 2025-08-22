using JustAnAiAgent.Data.Brokers.Interfaces;
using JustAnAiAgent.Objects.Entities;
using JustAnAiAgent.Services.Foundation.Interfaces;

namespace JustAnAiAgent.Services.Foundation;

public class MessageService(IMessageBroker broker) : IMessageService
{
    public IQueryable<Message> GetAll() =>
        broker.GetAll();

    public async ValueTask<Message> GetAsync(Guid id) =>
        await broker.GetAsync(id);

    public async ValueTask<Message> AddAsync(Message message)
    {
        message.CreatedAt = DateTimeOffset.UtcNow;

        return await broker.AddAsync(message);
    }

    public async ValueTask<Message> UpdateAsync(Guid id, Message message)
    {
        message.UpdatedAt = DateTimeOffset.UtcNow;

        return await broker.UpdateAsync(id, message);
    }

    public async Task DeleteAsync(Guid id) =>
        await broker.DeleteAsync(id);
}
