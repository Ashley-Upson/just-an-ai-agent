using System.Security.Authentication;
using System.Text.Json;
using JustAnAiAgent.MCP.Interfaces;
using JustAnAiAgent.MCP.MCP;
using JustAnAiAgent.Objects.Entities;
using JustAnAiAgent.Objects.Ollama;
using JustAnAiAgent.Services.Foundation;
using JustAnAiAgent.Services.Foundation.Interfaces;
using JustAnAiAgent.Services.Orchestration.Interfaces;
using JustAnAiAgent.Services.Processing.Interfaces;
using JustAnAiAjent.Objects.Providers;

namespace JustAnAiAgent.Services.Orchestration;

public class OllamaOrchestrationService(
    IConversationProcessingService conversationService,
    IMessageService messageService,
    IEnumerable<IMcpTool> tools,
    OllamaProviderService ollamaService) : IOllamaOrchestrationService
{
    public async ValueTask<Message> AddMessageAndSendToModel(Guid id, Message message)
    {
        Conversation conversation = await conversationService.GetWithMessagesAsync(id);

        if (conversation is null)
            throw new AuthenticationException("Access denied.");

        message.ConversationId = conversation.Id;

        Message dbMessage = await messageService.AddAsync(message);

        conversation.LastMessageSentAt = DateTimeOffset.UtcNow;
        await conversationService.UpdateAsync(conversation.Id, conversation);

        conversation.Messages.Add(dbMessage);

        string modelId = dbMessage.ModelId.Replace("<Ollama>", "");
        ProviderChatResponse response = await ollamaService.SendConversationToModelWithToolsAsync(modelId, conversation, tools.Select(t => t.GetToolDefinition()));

        Message currentMessage = await SaveResponseData(dbMessage, response);

        return await HandleModelResponse(conversation, currentMessage, response);
    }

    private async ValueTask<Message> SaveResponseData(Message message, ProviderChatResponse response)
    {
        if (response.thought is not null)
            message.ModelThought = response.thought;

        if (response.message is not null)
            message.ModelResponse = response.message;

        if (response.tool_calls is not null)
            message.ToolCalls = JsonSerializer.Serialize(response.tool_calls);
        else
        {
            // Because Ollama doesn't send all tool calls to the tool_calls property.
            try
            {
                string maybeJson = $"[{response.message}]";
                var attemptDeserialize = JsonSerializer.Deserialize<IEnumerable<OllamaToolCallFunction>>(maybeJson);

                if (attemptDeserialize is not null)
                {
                    var serialized = JsonSerializer.Serialize(attemptDeserialize.Select(c => new OllamaToolCall
                    {
                        function = c
                    }));

                    message.ToolCalls = serialized;
                    response.tool_calls = attemptDeserialize;
                }
            }
            catch { }
        }

        message.ResponseReceivedAt = DateTimeOffset.UtcNow;

        return await messageService.UpdateAsync(message.Id, message);
    }

    private async ValueTask<Message> HandleModelResponse(Conversation conversation, Message message, ProviderChatResponse response)
    {
        if (response.tool_calls is not null)
        {
            Dictionary<string, string> toolResponses = new Dictionary<string, string>();

            foreach (var call in response.tool_calls)
            {
                var tool = tools.FirstOrDefault(t => t.Name == call.function.name);

                if (tool is not null)
                    toolResponses.Add(call.function.name, await tool.Execute(ToolParameterInputsFromToolCallArguments(call.function.arguments)));
            }

            message.ToolResponses = JsonSerializer.Serialize(toolResponses);
            await messageService.UpdateAsync(message.Id, message);

            Message toolsResponseMessage = new Message()
            {
                ConversationId = conversation.Id,
                UserId = null,
                ModelId = message.ModelId,
                UserPrompt = null,
                SystemPrompt = "Here are the responses from the tools you requested. Please continue with the users request, or request more tool usage.",
                ModelThought = null,
                ModelResponse = null,
                ResponseReceivedAt = DateTimeOffset.UtcNow,
                ToolResponses = null
            };

            toolsResponseMessage = await messageService.AddAsync(toolsResponseMessage);
            conversation.Messages.Add(toolsResponseMessage);

            string modelId = message.ModelId.Replace("<Ollama>", "");
            ProviderChatResponse responseToToolsResults = await ollamaService.SendConversationToModelWithToolsAsync(modelId, conversation, tools.Select(t => t.GetToolDefinition()));

            Message currentMessage = await SaveResponseData(toolsResponseMessage, responseToToolsResults);

            if(responseToToolsResults.tool_calls is not null)
                await HandleModelResponse(conversation, currentMessage, responseToToolsResults);
        }

        return message;
    }

    private IEnumerable<ToolParameterInput> ToolParameterInputsFromToolCallArguments(Dictionary<string, string> arguments) =>
        arguments.Select(argument => new ToolParameterInput()
        {
            Name = argument.Key,
            Value = argument.Value,
        });
}