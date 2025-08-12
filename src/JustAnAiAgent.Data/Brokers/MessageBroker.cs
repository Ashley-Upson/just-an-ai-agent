using JustAnAiAgent.Data.Brokers.Interfaces;
using JustAnAiAgent.Data.Entities;
using JustAnAiAgent.Data.Interfaces;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace JustAnAiAgent.Data.Brokers;

public class MessageBroker(IJustAnAiAgentDbContextFactory contextFactory) : IMessageBroker
{
    public IQueryable<Message> GetAll()
    {
        using var context = contextFactory.CreateDbContext();

        IQueryable<Message> result = context.Messages;

        return result;
    }

    public async ValueTask<Message> Get(Guid id)
    {
        using var context = contextFactory.CreateDbContext();

        return await context.Messages.FindAsync(id);
    }

    public async ValueTask<Message> Add(Message message)
    {
        using var context = contextFactory.CreateDbContext();

        EntityEntry<Message> entry = await context.Messages.AddAsync(message);

        await context.SaveChangesAsync();

        return entry.Entity;
    }

    public async ValueTask<Message> Update(Message message)
    {
        using var context = contextFactory.CreateDbContext();

        EntityEntry<Message> entry = context.Messages.Update(message);

        await context.SaveChangesAsync();

        return entry.Entity;
    }

    public async void Delete(Message message)
    {
        using var context = contextFactory.CreateDbContext();

        context.Messages.Remove(new Message { Id = message.Id });

        await context.SaveChangesAsync();
    }
}