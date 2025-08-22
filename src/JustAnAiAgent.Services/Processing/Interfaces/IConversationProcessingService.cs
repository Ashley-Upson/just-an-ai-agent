using JustAnAiAgent.Objects.Entities;

namespace JustAnAiAgent.Services.Processing.Interfaces;

public interface IConversationProcessingService
{
    IQueryable<Conversation> GetAll();

    ValueTask<Conversation> GetAsync(Guid id);

    ValueTask<Conversation> GetWithMessagesAsync(Guid id);

    ValueTask<Conversation> AddAsync(Conversation conversation);

    ValueTask<Conversation> UpdateAsync(Guid id, Conversation conversation);

    Task DeleteAsync(Guid id);
}
