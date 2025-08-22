using JustAnAiAgent.Objects.Entities;

namespace JustAnAiAgent.Data.Brokers.Interfaces;

public interface IUserConversationBroker
{
    IQueryable<UserConversation> GetAll();

    ValueTask<UserConversation> AddAsync(UserConversation userConversation, bool ignoreAuth = false);

    Task DeleteAsync(string userId, Guid conversationId);
}