using JustAnAiAgent.Data.Brokers.Interfaces;
using JustAnAiAgent.Objects.Entities;
using JustAnAiAgent.Services.Foundation.Interfaces;

namespace JustAnAiAgent.Services.Foundation;

public class UserConversationService(IUserConversationBroker broker) : IUserConversationService
{
    public IQueryable<UserConversation> GetAll() =>
        broker.GetAll();

    public async ValueTask<UserConversation> AddAsync(UserConversation userConversation, bool ignoreAuth = false)
    {
        userConversation.CreatedAt = DateTimeOffset.UtcNow;

        return await broker.AddAsync(userConversation, ignoreAuth);
    }

    public async Task DeleteAsync(string userId, Guid conversationId) =>
        await broker.DeleteAsync(userId, conversationId);
}
