using System.Security.Authentication;
using cCoder.Security.Objects;
using JustAnAiAgent.Data.Brokers.Interfaces;
using JustAnAiAgent.Objects.Entities;
using JustAnAiAgent.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace JustAnAiAgent.Data.Brokers;

public class MessageBroker(
    IJustAnAiAgentDbContextFactory contextFactory,
    ISSOAuthInfo authInfo) : IMessageBroker
{
    public IQueryable<Message> GetAll()
    {
        var context = contextFactory.CreateDbContext();

        IQueryable<Message> result = context.Messages
            .Where(m => m.Conversation.Users.Any(uc => uc.UserId == authInfo.SSOUserId));

        return result;
    }

    public async ValueTask<Message> GetAsync(Guid id)
    {
        using var context = contextFactory.CreateDbContext();

        return await context.Messages
            .Where(m => m.Conversation.Users.Any(uc => uc.UserId == authInfo.SSOUserId))
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async ValueTask<Message> AddAsync(Message message)
    {
        using var context = contextFactory.CreateDbContext();

        Conversation conversation = await context.Conversations
            .Where(c => c.Users.Any(u => u.UserId == authInfo.SSOUserId))
            .FirstOrDefaultAsync(c => c.Id == message.ConversationId);

        if(conversation is null)
            throw new AuthenticationException("Access denied.");

        message.UserId = authInfo.SSOUserId;

        EntityEntry<Message> entry = await context.Messages.AddAsync(message);

        await context.SaveChangesAsync();

        return entry.Entity;
    }

    public async ValueTask<Message> UpdateAsync(Guid id, Message message)
    {
        using var context = contextFactory.CreateDbContext();

        Message dbMessage = await GetAsync(id);

        if (dbMessage is null)
            throw new AuthenticationException("Access denied.");

        Message update = new()
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            UserId = message.UserId,
            ModelId = message.ModelId,
            UserPrompt = message.UserPrompt,
            SystemPrompt = message.SystemPrompt,
            ModelThought = message.ModelThought,
            ModelResponse = message.ModelResponse,
            ResponseReceivedAt = message.ResponseReceivedAt,
            ToolCalls = message.ToolCalls,
            ToolResponses = message.ToolResponses,
            CreatedAt = message.CreatedAt,
            UpdatedAt = message.UpdatedAt
        };

        EntityEntry<Message> entry = context.Messages.Update(update);

        await context.SaveChangesAsync();

        return entry.Entity;
    }

    public async Task DeleteAsync(Guid id)
    {
        using var context = contextFactory.CreateDbContext();

        Message dbMessage = await GetAsync(id);

        if (dbMessage is null)
            throw new AuthenticationException("Access denied.");

        context.Messages.Remove(new Message { Id = id });

        await context.SaveChangesAsync();
    }
}