using System.Security.Authentication;
using JustAnAiAgent.Objects.Entities;
using JustAnAiAgent.Services.Foundation;
using JustAnAiAgent.Services.Foundation.Interfaces;
using JustAnAiAgent.Services.Orchestration.Interfaces;
using JustAnAiAgent.Services.Processing.Interfaces;
using JustAnAiAjent.Objects.Providers;

namespace JustAnAiAgent.Services.Orchestration;

public class OllamaOrchestrationService(
    IConversationProcessingService conversationService,
    IMessageService messageService,
    OllamaProviderService ollamaService) : IOllamaOrchestrationService
{
    public async ValueTask<Message> AddMessageAndSendToModel(Guid id, Message message)
    {
        Conversation conversation = await conversationService.GetWithMessagesAsync(id);

        if (conversation is null)
            throw new AuthenticationException("Access denied.");

        message.ConversationId = conversation.Id;

        Message dbMessage = await messageService.AddAsync(message);

        conversation.Messages.Add(dbMessage);

        var modelId = dbMessage.ModelId.Replace("<Ollama>", "");
        ProviderChatResponse response = await ollamaService.SendConversationToModelAsync(modelId, conversation);

        if (response.thought is not null)
            dbMessage.ModelThought = response.thought;

        if (response.message is not null)
            dbMessage.ModelResponse = response.message;

        dbMessage.ResponseReceivedAt = DateTimeOffset.UtcNow;

        return await messageService.UpdateAsync(message.Id, dbMessage);
    }
}