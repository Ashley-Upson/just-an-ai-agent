using System.Security.Authentication;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using JustAnAiAgent.MCP.Interfaces;
using JustAnAiAgent.MCP.MCP;
using JustAnAiAgent.Objects.Entities;
using JustAnAiAgent.Objects.Ollama;
using JustAnAiAgent.Objects.Providers;
using JustAnAiAgent.Services.Foundation.Interfaces;
using JustAnAiAgent.Services.Orchestration.Interfaces;
using JustAnAiAgent.Services.Processing.Interfaces;

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

    public async IAsyncEnumerable<Message> AddMessageAndSendToModelStream(
        Guid id,
        Message message,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        Conversation conversation = await conversationService.GetWithMessagesAsync(id);

        if (conversation is null)
            throw new AuthenticationException("Access denied.");

        message.ConversationId = conversation.Id;

        Message dbMessage = await messageService.AddAsync(message);

        conversation.LastMessageSentAt = DateTimeOffset.UtcNow;
        await conversationService.UpdateAsync(conversation.Id, conversation);

        conversation.Messages.Add(dbMessage);

        await foreach (Message streamedMessage in StreamModelResponse(conversation, dbMessage, cancellationToken))
            yield return streamedMessage;
    }

    private async IAsyncEnumerable<Message> StreamModelResponse(
        Conversation conversation,
        Message triggerMessage,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        StringBuilder accumulatedResponse = new();
        StringBuilder accumulatedThought = new();
        int responseChunkCount = 0;
        int thoughtChunkCount = 0;

        Message responseMessage = null;
        Message thoughtMessage = null;
        IEnumerable<OllamaToolCall> toolCallsFromStream = null;

        await foreach (ProviderChatStreamChunk chunk in llmProviderService.SendConversationToModelWithToolsStreamAsync(
            triggerMessage.ModelId,
            conversation,
            tools.Select(t => t.GetToolDefinition()),
            cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!string.IsNullOrWhiteSpace(chunk.thought))
            {
                accumulatedThought.Append(chunk.thought);
                thoughtChunkCount++;

                if (thoughtMessage is null)
                {
                    thoughtMessage = await messageService.AddAsync(new()
                    {
                        ConversationId = triggerMessage.ConversationId,
                        ModelId = triggerMessage.ModelId,
                        UserId = triggerMessage.UserId,
                        Type = "thought",
                        ContentType = "string",
                        Content = accumulatedThought.ToString(),
                        IsComplete = false,
                        IsStillRunning = true
                    });

                    conversation.Messages.Add(thoughtMessage);
                }
                else if (thoughtChunkCount % 5 == 0)
                {
                    thoughtMessage.Content = accumulatedThought.ToString();
                    thoughtMessage.IsComplete = false;
                    thoughtMessage.IsStillRunning = true;
                    await messageService.UpdateAsync(thoughtMessage.Id, thoughtMessage);
                }

                yield return SnapshotMessage(thoughtMessage, accumulatedThought.ToString(), false, true);
            }

            if (!string.IsNullOrWhiteSpace(chunk.message))
            {
                accumulatedResponse.Append(chunk.message);
                responseChunkCount++;

                if (responseMessage is null)
                {
                    responseMessage = await messageService.AddAsync(new()
                    {
                        ConversationId = triggerMessage.ConversationId,
                        ModelId = triggerMessage.ModelId,
                        UserId = triggerMessage.UserId,
                        Type = "response",
                        ContentType = "string",
                        Content = accumulatedResponse.ToString(),
                        IsComplete = false,
                        IsStillRunning = true
                    });

                    conversation.Messages.Add(responseMessage);
                }
                else if (responseChunkCount % 5 == 0)
                {
                    responseMessage.Content = accumulatedResponse.ToString();
                    responseMessage.IsComplete = false;
                    responseMessage.IsStillRunning = true;
                    await messageService.UpdateAsync(responseMessage.Id, responseMessage);
                }

                yield return SnapshotMessage(responseMessage, accumulatedResponse.ToString(), false, true);
            }

            if (chunk.tool_calls is IEnumerable<OllamaToolCall> toolCalls)
                toolCallsFromStream = toolCalls;
        }

        if (thoughtMessage is not null)
        {
            thoughtMessage.Content = accumulatedThought.ToString();
            thoughtMessage.IsComplete = true;
            thoughtMessage.IsStillRunning = false;
            thoughtMessage.ResponseReceivedAt = DateTimeOffset.UtcNow;
            await messageService.UpdateAsync(thoughtMessage.Id, thoughtMessage);
            yield return SnapshotMessage(thoughtMessage, thoughtMessage.Content, true, false);
        }

        if (responseMessage is not null)
        {
            responseMessage.Content = accumulatedResponse.ToString();
            responseMessage.IsComplete = true;
            responseMessage.IsStillRunning = false;
            responseMessage.ResponseReceivedAt = DateTimeOffset.UtcNow;
            await messageService.UpdateAsync(responseMessage.Id, responseMessage);
            yield return SnapshotMessage(responseMessage, responseMessage.Content, true, false);
        }

        if (toolCallsFromStream is not null && toolCallsFromStream.Any())
        {
            Message toolCallsMessage = await messageService.AddAsync(new()
            {
                ConversationId = triggerMessage.ConversationId,
                ModelId = triggerMessage.ModelId,
                UserId = triggerMessage.UserId,
                Type = "tool-calls",
                ContentType = "json",
                Content = JsonSerializer.Serialize(toolCallsFromStream),
                IsComplete = true,
                IsStillRunning = false,
                ResponseReceivedAt = DateTimeOffset.UtcNow,
            });

            conversation.Messages.Add(toolCallsMessage);
            yield return SnapshotMessage(toolCallsMessage, toolCallsMessage.Content, true, false);

            Dictionary<string, string> toolResponses = new();

            foreach (OllamaToolCall call in toolCallsFromStream)
            {
                IMcpTool tool = tools.FirstOrDefault(t => t.Name == call.function.name);

                if (tool is not null)
                    toolResponses.Add(call.function.name, await tool.Execute(ToolParameterInputsFromToolCallArguments(call.function.arguments)));
            }

            Message toolResultsMessage = await messageService.AddAsync(new()
            {
                ConversationId = triggerMessage.ConversationId,
                ModelId = triggerMessage.ModelId,
                UserId = triggerMessage.UserId,
                Type = "tool-results",
                ContentType = "json",
                Content = JsonSerializer.Serialize(toolResponses),
                IsComplete = true,
                IsStillRunning = false,
                ResponseReceivedAt = DateTimeOffset.UtcNow,
            });

            conversation.Messages.Add(toolResultsMessage);
            yield return SnapshotMessage(toolResultsMessage, toolResultsMessage.Content, true, false);

            await foreach (Message recursiveStreamMessage in StreamModelResponse(conversation, toolResultsMessage, cancellationToken))
                yield return recursiveStreamMessage;
        }
    }

    private async ValueTask<IEnumerable<Message>> SaveResponseData(Message message, ProviderChatResponse response)
    {
        List<Message> messages = [];

        if(!string.IsNullOrWhiteSpace(response.thought))
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
                IsComplete = true,
                IsStillRunning = false,
            }));
        }

        if (!string.IsNullOrWhiteSpace(response.message))
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
                IsComplete = true,
                IsStillRunning = false,
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
                Content = JsonSerializer.Serialize(response.tool_calls),
                IsComplete = true,
                IsStillRunning = false,
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
                        Content = serialized,
                        IsComplete = true,
                        IsStillRunning = false,
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
                Content = "{\"still-executing\":true}",
                IsComplete = false,
                IsStillRunning = true,
            });

            foreach (var call in response.tool_calls)
            {
                var tool = tools.FirstOrDefault(t => t.Name == call.function.name);

                if (tool is not null)
                    toolResponses.Add(call.function.name, await tool.Execute(ToolParameterInputsFromToolCallArguments(call.function.arguments)));
            }

            toolResults.Content = JsonSerializer.Serialize(toolResponses);
            toolResults.ResponseReceivedAt = DateTimeOffset.UtcNow;
            toolResults.IsComplete = true;
            toolResults.IsStillRunning = false;
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

    private static Message SnapshotMessage(Message message, string content, bool isComplete, bool isStillRunning)
    {
        return new Message
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            UserId = message.UserId,
            Type = message.Type,
            ModelId = message.ModelId,
            Content = content,
            ContentType = message.ContentType,
            ResponseReceivedAt = message.ResponseReceivedAt,
            CreatedAt = message.CreatedAt,
            UpdatedAt = message.UpdatedAt,
            IsComplete = isComplete,
            IsStillRunning = isStillRunning,
        };
    }
}
