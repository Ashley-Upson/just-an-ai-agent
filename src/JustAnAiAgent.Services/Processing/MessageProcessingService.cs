using JustAnAiAgent.Objects.Entities;
using JustAnAiAgent.Services.Foundation.Interfaces;
using JustAnAiAgent.Services.Processing.Interfaces;

namespace JustAnAiAgent.Services.Processing;

public class MessageProcessingService(
    IMessageService service,
    IConversationService conversationService) : IMessageProcessingService
{
    public IQueryable<Message> GetAll() =>
        service.GetAll();

    public async ValueTask<Message> GetAsync(Guid id) =>
        await service.GetAsync(id);

    public async ValueTask<Message> AddAsync(Message message)
    {    
        var dbMessage = await service.AddAsync(message);
        var conversation = await conversationService.GetAsync(message.ConversationId);

        conversation.LastMessageSentAt = DateTimeOffset.UtcNow;
        await conversationService.UpdateAsync(conversation.Id, conversation);

        return dbMessage;
    }

    public async ValueTask<Message> UpdateAsync(Guid id, Message message) =>
        await service.UpdateAsync(id, message);

    public async Task DeleteAsync(Guid id) =>
        await service.DeleteAsync(id);
}