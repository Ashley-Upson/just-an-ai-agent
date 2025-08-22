using JustAnAiAgent.Data.Brokers.Interfaces;
using JustAnAiAgent.Objects.Entities;
using JustAnAiAgent.Services.Foundation.Interfaces;

namespace JustAnAiAgent.Services.Foundation;

public class ConversationService(IConversationBroker broker) : IConversationService
{
    public IQueryable<Conversation> GetAll() =>
        broker.GetAll();

    public async ValueTask<Conversation> GetAsync(Guid id) =>
        await broker.GetAsync(id);

    public async ValueTask<Conversation> GetWithMessagesAsync(Guid id) =>
        await broker.GetWithMessagesAsync(id);

    public async ValueTask<Conversation> AddAsync(Conversation conversation)
    {
        conversation.CreatedAt = DateTimeOffset.UtcNow;

        return await broker.AddAsync(conversation);
    }

    public async ValueTask<Conversation> UpdateAsync(Guid id, Conversation conversation)
    {
        conversation.UpdatedAt = DateTimeOffset.UtcNow;

        return await broker.UpdateAsync(id, conversation);
    }

    public async Task DeleteAsync(Guid id, bool ignoreAuth = false) =>
        await broker.DeleteAsync(id, ignoreAuth);
}
