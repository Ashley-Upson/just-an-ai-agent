using JustAnAiAgent.Objects.Entities;
using JustAnAiAgent.Providers;
using JustAnAiAjent.Objects.Providers;

namespace JustAnAiAgent.Services.Orchestration.Interfaces;

public interface IOllamaOrchestrationService
{
    ValueTask<Message> AddMessageAndSendToModel(Guid id, Message message);
}