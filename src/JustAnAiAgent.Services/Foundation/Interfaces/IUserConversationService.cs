using JustAnAiAgent.Data.Entities;

namespace JustAnAiAgent.Data.Brokers.Interfaces;

interface IUserConversationService
{
    IQueryable<UserConversation> GetAll();

    ValueTask<UserConversation> Add(UserConversation userConversation);

    void Delete(UserConversation userConversation);
}