using JustAnAiAgent.Data.Brokers.Interfaces;
using JustAnAiAgent.Objects.Entities;
using JustAnAiAgent.Data.Interfaces;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using System.Security.Authentication;
using cCoder.Security.Objects;

namespace JustAnAiAgent.Data.Brokers;

public class ConversationBroker(
    IJustAnAiAgentDbContextFactory contextFactory,
    ISSOAuthInfo authInfo) : IConversationBroker
{
    public IQueryable<Conversation> GetAll()
    {
        var context = contextFactory.CreateDbContext();

        IQueryable<Conversation> result = context.Conversations
            .Where(c => c.Users.Any(u => u.UserId == authInfo.SSOUserId));

        return result;
    }

    public async ValueTask<Conversation> GetAsync(Guid id)
    {
        using var context = contextFactory.CreateDbContext();

        return await context.Conversations
            .Where(c => c.Users.Any(u => u.UserId == authInfo.SSOUserId))
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async ValueTask<Conversation> GetWithMessagesAsync(Guid id)
    {
        using var context = contextFactory.CreateDbContext();

        return await context.Conversations
            .Where(c => c.Users.Any(u => u.UserId == authInfo.SSOUserId))
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async ValueTask<Conversation> AddAsync(Conversation conversation)
    {
        using var context = contextFactory.CreateDbContext();
        
        EntityEntry<Conversation> entry = await context.Conversations.AddAsync(conversation);
        
        await context.SaveChangesAsync();

        return entry.Entity;
    }

    public async ValueTask<Conversation> UpdateAsync(Guid id, Conversation conversation)
    {
        using var context = contextFactory.CreateDbContext();

        var existing = await GetAsync(id);

        if (existing is null)
            throw new AuthenticationException("Access denied.");

        conversation.Id = id;

        EntityEntry<Conversation> entry = context.Conversations.Update(conversation);

        await context.SaveChangesAsync();

        return entry.Entity;
    }

    public async Task DeleteAsync(Guid id, bool ignoreAuth = false)
    {
        using var context = contextFactory.CreateDbContext();

        if (!ignoreAuth)
        {
            var existing = await GetAsync(id);

            if (existing is null)
                throw new AuthenticationException("Access denied.");
        }

        context.Conversations.Remove(new Conversation { Id = id });

        await context.SaveChangesAsync();
    }
}