using JustAnAiAgent.Objects.Entities;

namespace JustAnAiAgent.Data.Brokers.Interfaces;

public interface IConversationBroker
{
    IQueryable<Conversation> GetAll();

    ValueTask<Conversation> GetAsync(Guid id);

    ValueTask<Conversation> GetWithMessagesAsync(Guid id);

    ValueTask<Conversation> AddAsync(Conversation conversation);

    ValueTask<Conversation> UpdateAsync(Guid id, Conversation conversation);

    Task DeleteAsync(Guid id, bool ignoreAuth = false);
}