using JustAnAiAgent.Objects.Entities;

namespace JustAnAiAgent.Services.Foundation.Interfaces;

public interface IUserConversationService
{
    IQueryable<UserConversation> GetAll();

    ValueTask<UserConversation> AddAsync(UserConversation userConversation, bool ignoreAuth = false);

    Task DeleteAsync(string userId, Guid conversationId);
}