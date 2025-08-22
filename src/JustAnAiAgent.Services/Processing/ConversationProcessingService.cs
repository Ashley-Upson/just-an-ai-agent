using cCoder.Security.Objects;
using JustAnAiAgent.Objects.Entities;
using JustAnAiAgent.Services.Foundation.Interfaces;
using JustAnAiAgent.Services.Processing.Interfaces;

namespace JustAnAiAgent.Services.Processing;

public class ConversationProcessingService(
    IConversationService service,
    IUserConversationService userConversationService,
    IAgenticProjectService agenticProjectService,
    IMessageService messageService,
    ISSOAuthInfo authInfo) : IConversationProcessingService
{
    public IQueryable<Conversation> GetAll() =>
        service.GetAll();

    public async ValueTask<Conversation> GetAsync(Guid id) =>
        await service.GetAsync(id);

    public async ValueTask<Conversation> GetWithMessagesAsync(Guid id) =>
        await service.GetWithMessagesAsync(id);

    public async ValueTask<Conversation> AddAsync(Conversation conversation)
    {
        Conversation added = await service.AddAsync(conversation);

        await userConversationService.AddAsync(new()
        {
            UserId = authInfo.SSOUserId,
            ConversationId = conversation.Id
        }, true);

        return added;
    }

    public async ValueTask<Conversation> UpdateAsync(Guid id, Conversation conversation) =>
        await service.UpdateAsync(id, conversation);

    public async Task DeleteAsync(Guid id)
    {
        IEnumerable<Message> messages = messageService.GetAll()
            .Where(m => m.ConversationId == id);

        IEnumerable<AgenticProject> projects = agenticProjectService.GetAll()
            .Where(ap => messages.Select(m => m.Id).Contains(ap.MessageId));

        IEnumerable<UserConversation> users = userConversationService.GetAll()
            .Where(uc => uc.ConversationId == id);

        foreach(AgenticProject project in projects)
            await agenticProjectService.DeleteAsync(project.Id);

        foreach(Message message in messages)
            await messageService.DeleteAsync(message.Id);

        foreach (UserConversation user in users)
            await userConversationService.DeleteAsync(user.UserId, id);

        await service.DeleteAsync(id, true);
    }
}