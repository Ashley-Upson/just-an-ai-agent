using JustAnAiAgent.Data.Entities;

namespace JustAnAiAgent.Services.Foundation.Interfaces;

interface IConversationService
{
    IQueryable<Conversation> GetAll();

    ValueTask<Conversation> Get(Guid id);

    ValueTask<Conversation> Add(Conversation conversation);

    ValueTask<Conversation> Update(Conversation conversation);

    void Delete(Conversation conversation);
}