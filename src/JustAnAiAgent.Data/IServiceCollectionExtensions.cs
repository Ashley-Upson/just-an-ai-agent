using JustAnAiAgent.Data.Brokers;
using JustAnAiAgent.Data.Brokers.Interfaces;
using JustAnAiAgent.Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace JustAnAiAgent.Data;

public static class IServiceCollectionExtensions
{
    public static void AddDataServices(this IServiceCollection services)
    {
        services.AddScoped<IJustAnAiAgentDbContextFactory, MSSQLJustAnAiAgentDbContextFactory>();

        // Brokers.
        services.AddTransient<IConversationBroker, ConversationBroker>();
        services.AddTransient<IMessageBroker, MessageBroker>();
        services.AddTransient<IUserConversationBroker, UserConversationBroker>();
        services.AddTransient<IAgenticProjectBroker, AgenticProjectBroker>();
        services.AddTransient<IProjectTaskBroker, ProjectTaskBroker>();

        // Model provider brokers.
        services.AddScoped<OllamaProviderBroker>();
        services.AddScoped<ILLMProviderBroker, OllamaProviderBroker>();
    }
}
