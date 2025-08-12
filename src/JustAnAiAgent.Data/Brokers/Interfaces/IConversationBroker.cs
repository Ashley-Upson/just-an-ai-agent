using JustAnAiAgent.Data.Entities;

namespace JustAnAiAgent.Data.Brokers.Interfaces;

interface IConversationBroker
{
    IQueryable<Conversation> GetAll();
    
    ValueTask<Conversation> Get(Guid id);

    ValueTask<Conversation> Add(Conversation conversation);

    ValueTask<Conversation> Update(Conversation conversation);

    void Delete(Conversation conversation);
}