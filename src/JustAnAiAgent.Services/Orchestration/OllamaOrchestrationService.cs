using System.Security.Authentication;
using System.Text.Json;
using JustAnAiAgent.MCP.Interfaces;
using JustAnAiAgent.MCP.MCP;
using JustAnAiAgent.Objects.Entities;
using JustAnAiAgent.Objects.Ollama;
using JustAnAiAgent.Services.Foundation.Interfaces;
using JustAnAiAgent.Services.Orchestration.Interfaces;
using JustAnAiAgent.Services.Processing.Interfaces;
using JustAnAiAgent.Objects.Providers;

namespace JustAnAiAgent.Services.Orchestration;

public class OllamaOrchestrationService(
    IConversationProcessingService conversationService,
    IMessageService messageService,
    IEnumerable<IMcpTool> tools,
    ILLMProviderService llmProviderService) : IOllamaOrchestrationService
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

        ProviderChatResponse response = await llmProviderService.SendConversationToModelWithToolsAsync(dbMessage.ModelId, conversation, tools.Select(t => t.GetToolDefinition()));

        IEnumerable<Message> messages = await SaveResponseData(dbMessage, response);

        return await HandleModelResponse(conversation, messages.Last(), response);
    }

    private async ValueTask<IEnumerable<Message>> SaveResponseData(Message message, ProviderChatResponse response)
    {
        List<Message> messages = [];

        if(response.thought is not null)
        {
            messages.Add(await messageService.AddAsync(new()
            {
                ConversationId = message.ConversationId,
                ModelId = message.ModelId,
                UserId = message.UserId,
                Type = "thought",
                ContentType = "string",
                Content = response.thought,
                ResponseReceivedAt = DateTimeOffset.UtcNow,
            }));
        }

        if (response.message is not null)
        {
            messages.Add(await messageService.AddAsync(new()
            {
                ConversationId = message.ConversationId,
                ModelId = message.ModelId,
                UserId = message.UserId,
                Type = "response",
                ContentType = "string",
                Content = response.message,
                ResponseReceivedAt = DateTimeOffset.UtcNow,
            }));
        }

        if (response.tool_calls is not null)
        {
            messages.Add(await messageService.AddAsync(new()
            {
                ConversationId = message.ConversationId,
                ModelId = message.ModelId,
                UserId = message.UserId,
                Type = "tool-calls",
                ContentType = "json",
                Content = JsonSerializer.Serialize(response.tool_calls)
            }));
        }
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

                    messages.Add(await messageService.AddAsync(new()
                    {
                        ConversationId = message.ConversationId,
                        ModelId = message.ModelId,
                        UserId = message.UserId,
                        Type = "tool-calls",
                        ContentType = "json",
                        Content = serialized
                    }));
                }
            }
            catch { }
        }

        return messages;
    }

    private async ValueTask<Message> HandleModelResponse(Conversation conversation, Message message, ProviderChatResponse response)
    {
        if (response.tool_calls is not null)
        {
            Dictionary<string, string> toolResponses = new Dictionary<string, string>();

            Message toolResults = await messageService.AddAsync(new()
            {
                ConversationId = message.ConversationId,
                ModelId = message.ModelId,
                UserId = message.UserId,
                Type = "tool-results",
                ContentType = "json",
            });

            foreach (var call in response.tool_calls)
            {
                var tool = tools.FirstOrDefault(t => t.Name == call.function.name);

                if (tool is not null)
                    toolResponses.Add(call.function.name, await tool.Execute(ToolParameterInputsFromToolCallArguments(call.function.arguments)));
            }

            toolResults.Content = JsonSerializer.Serialize(toolResponses);
            toolResults.ResponseReceivedAt = DateTimeOffset.UtcNow;
            await messageService.UpdateAsync(toolResults.Id, toolResults);

            conversation.Messages.Add(toolResults);

            ProviderChatResponse responseToToolsResults = await llmProviderService.SendConversationToModelWithToolsAsync(message.ModelId, conversation, tools.Select(t => t.GetToolDefinition()));

            IEnumerable<Message> newMessages = await SaveResponseData(toolResults, responseToToolsResults);

            if(responseToToolsResults.tool_calls is not null)
                await HandleModelResponse(conversation, newMessages.Last(), responseToToolsResults);
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
