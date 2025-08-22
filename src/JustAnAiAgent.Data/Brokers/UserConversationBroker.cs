using System.Security.Authentication;
using cCoder.Security.Data.Brokers.Utility.Interfaces;
using cCoder.Security.Objects;
using JustAnAiAgent.Data.Brokers.Interfaces;
using JustAnAiAgent.Objects.Entities;
using JustAnAiAgent.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace JustAnAiAgent.Data.Brokers;

public class UserConversationBroker(
    IJustAnAiAgentDbContextFactory contextFactory,
    ISSOAuthInfo authInfo) : IUserConversationBroker
{
    public IQueryable<UserConversation> GetAll()
    {
        var context = contextFactory.CreateDbContext();

        IQueryable<UserConversation> result = context.UserConversations
                .Where(c => c.UserId == authInfo.SSOUserId);

        return result;
    }

    public async ValueTask<UserConversation> AddAsync(UserConversation userConversation, bool ignoreAuth = false)
    {
        using var context = contextFactory.CreateDbContext();

        if(!ignoreAuth)
        {
            Conversation conversation = await context.Conversations
                .Where(c => c.Users.Any(cu => cu.UserId == authInfo.SSOUserId))
                .FirstOrDefaultAsync(c => c.Id == userConversation.ConversationId);

            if (conversation is null)
                throw new AuthenticationException("Access denied.");
        }
        
        EntityEntry<UserConversation> entry = await context.UserConversations.AddAsync(userConversation);
        
        await context.SaveChangesAsync();

        return entry.Entity;
    }

    public async Task DeleteAsync(string userId, Guid conversationId)
    {
        using var context = contextFactory.CreateDbContext();

        Conversation conversation = await context.Conversations
            .Where(c => c.Users.Any(cu => cu.UserId == authInfo.SSOUserId))
            .FirstOrDefaultAsync(c => c.Id == conversationId);

        if (conversation is null)
            throw new AuthenticationException("Access denied.");

        context.UserConversations.Remove(new UserConversation {
            UserId = userId,
            ConversationId = conversationId
        });

        await context.SaveChangesAsync();
    }
}