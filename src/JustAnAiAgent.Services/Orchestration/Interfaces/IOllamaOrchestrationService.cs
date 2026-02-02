using JustAnAiAgent.Objects.Entities;

namespace JustAnAiAgent.Services.Orchestration.Interfaces;

public interface IOllamaOrchestrationService
{
    ValueTask<Message> AddMessageAndSendToModel(Guid id, Message message);
}
