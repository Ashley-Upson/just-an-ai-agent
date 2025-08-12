using JustAnAiAgent.Data.Brokers.Interfaces;
using JustAnAiAgent.Data.Entities;
using JustAnAiAgent.Data.Interfaces;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace JustAnAiAgent.Data.Brokers;

public class ConversationBroker(IJustAnAiAgentDbContextFactory contextFactory) : IConversationBroker
{
    public IQueryable<Conversation> GetAll()
    {
        using var context = contextFactory.CreateDbContext();

        IQueryable<Conversation> result = context.Conversations;

        return result;
    }

    public async ValueTask<Conversation> Get(Guid id)
    {
        using var context = contextFactory.CreateDbContext();

        return await context.Conversations.FindAsync(id);
    }

    public async ValueTask<Conversation> Add(Conversation conversation)
    {
        using var context = contextFactory.CreateDbContext();
        
        EntityEntry<Conversation> entry = await context.Conversations.AddAsync(conversation);
        
        await context.SaveChangesAsync();

        return entry.Entity;
    }

    public async ValueTask<Conversation> Update(Conversation conversation)
    {
        using var context = contextFactory.CreateDbContext();

        EntityEntry<Conversation> entry = context.Conversations.Update(conversation);

        await context.SaveChangesAsync();

        return entry.Entity;
    }

    public async void Delete(Conversation conversation)
    {
        using var context = contextFactory.CreateDbContext();

        context.Conversations.Remove(new Conversation { Id = conversation.Id });

        await context.SaveChangesAsync();
    }
}