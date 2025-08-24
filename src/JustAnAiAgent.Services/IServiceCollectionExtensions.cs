using JustAnAiAgent.Services.Foundation;
using JustAnAiAgent.Services.Foundation.Interfaces;
using JustAnAiAgent.Services.Orchestration;
using JustAnAiAgent.Services.Orchestration.Interfaces;
using JustAnAiAgent.Services.Processing;
using JustAnAiAgent.Services.Processing.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace JustAnAiAgent.Services;

public static class IServiceCollectionExtensions
{
    public static void AddStandardServices(this IServiceCollection services)
    {
        // Foundation services.
        services.AddTransient<IConversationService, ConversationService>();
        services.AddTransient<IMessageService, MessageService>();
        services.AddTransient<IUserConversationService, UserConversationService>();
        services.AddTransient<IAgenticProjectService, AgenticProjectService>();
        services.AddTransient<IProjectTaskService, ProjectTaskService>();

        // Provider foundation services.
        services.AddScoped<OllamaProviderService>();
        services.AddScoped<ILLMProviderService, OllamaProviderService>();

        // Processing services.
        services.AddTransient<IConversationProcessingService, ConversationProcessingService>();
        services.AddTransient<IMessageProcessingService, MessageProcessingService>();

        // Provider processing services.

        // Provider orchestration services.
        services.AddTransient<IOllamaOrchestrationService, OllamaOrchestrationService>();
    }
}