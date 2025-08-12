using JustAnAiAgent.Data.Entities;

namespace JustAnAiAgent.Services.Foundation.Interfaces;

interface IMessageService
{
    IQueryable<Message> GetAll();

    ValueTask<Message> Get(Guid id);

    ValueTask<Message> Add(Message message);

    ValueTask<Message> Update(Message message);

    void Delete(Message message);
}