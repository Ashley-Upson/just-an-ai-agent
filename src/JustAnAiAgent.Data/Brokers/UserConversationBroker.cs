using JustAnAiAgent.Data.Brokers.Interfaces;
using JustAnAiAgent.Data.Entities;
using JustAnAiAgent.Data.Interfaces;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace JustAnAiAgent.Data.Brokers;

public class UserConversationBroker(IJustAnAiAgentDbContextFactory contextFactory) : IUserConversationBroker
{
    public IQueryable<UserConversation> GetAll()
    {
        using var context = contextFactory.CreateDbContext();

        IQueryable<UserConversation> result = context.UserConversations;

        return result;
    }

    public async ValueTask<UserConversation> Add(UserConversation userConversation)
    {
        using var context = contextFactory.CreateDbContext();
        
        EntityEntry<UserConversation> entry = await context.UserConversations.AddAsync(userConversation);
        
        await context.SaveChangesAsync();

        return entry.Entity;
    }

    public async void Delete(UserConversation userConversation)
    {
        using var context = contextFactory.CreateDbContext();

        context.UserConversations.Remove(new UserConversation {
            UserId = userConversation.UserId,
            ConversationId = userConversation.ConversationId
        });

        await context.SaveChangesAsync();
    }
}