using System.Net.Http.Json;
using System.Text.Json;
using JustAnAiAgent.MCP.MCP;
using JustAnAiAgent.Objects.Entities;
using JustAnAiAgent.Objects.Ollama;
using JustAnAiAjent.Objects.Ollama;
using JustAnAiAjent.Objects.Providers;

namespace JustAnAiAgent.Providers.Ollama;

class OllamaClient
{
    private string ApiUrl {  get; set; }

    private int Port { get; set; }

    private HttpClient ApiClient { get; set; }

    public OllamaClient(string url, int port)
    {
        ApiUrl = url;
        Port = port;

        ApiClient = new()
        {
            BaseAddress = new Uri($"{ApiUrl}:{Port}/api/"),
            Timeout = Timeout.InfiniteTimeSpan
        };
    }

    public async ValueTask<OllamaResponse> SendChatMessageAsync(ProviderChatRequest request)
    {
        OllamaRequest ollamaRequest = BuildBasicChatRequest(request);

        HttpResponseMessage httpResponse = await ApiClient.PostAsJsonAsync("chat", ollamaRequest);
        httpResponse.EnsureSuccessStatusCode();
        OllamaResponse response = await httpResponse.Content.ReadFromJsonAsync<OllamaResponse>();

        return response;
    }

    public async ValueTask<OllamaTagsResponse> GetAvailableModelsAsync()
    {
        HttpResponseMessage httpResponse = await ApiClient.GetAsync("tags");
        httpResponse.EnsureSuccessStatusCode();
        return await httpResponse.Content.ReadFromJsonAsync<OllamaTagsResponse>();
    }

    public async ValueTask<OllamaResponse> SendChatMessageWithToolsAsync(ProviderChatRequest request, IEnumerable<ToolDefinition> tools)
    {
        OllamaRequest ollamaRequest = BuildChatRequestWithTools(request, tools);

        HttpResponseMessage httpResponse = await ApiClient.PostAsJsonAsync("chat", ollamaRequest);
        httpResponse.EnsureSuccessStatusCode();
        OllamaResponse response = await httpResponse.Content.ReadFromJsonAsync<OllamaResponse>();

        return response;
    }

    private OllamaRequest BuildBasicChatRequest(ProviderChatRequest request)
    {
        OllamaRequest ollamaRequest = new();
        ollamaRequest.stream = false;
        ollamaRequest.options = new()
        {
            { "num_ctx", 20000 },
            { "num_thread", 20 },
            { "num_gpu", 16 }
        };
        List<OllamaMessage> messages = new();

        foreach (var message in request.messages)
            messages.AddRange(OllamaMessagesFromMessage(message));

        ollamaRequest.messages = messages;
        ollamaRequest.model = request.model;
        return ollamaRequest;
    }

    private OllamaRequest BuildChatRequestWithTools(ProviderChatRequest request, IEnumerable<ToolDefinition> tools)
    {
        OllamaRequest ollamaRequest = new();
        ollamaRequest.stream = false;
        List<OllamaMessage> messages = new();

        foreach (var message in request.messages)
            messages.AddRange(OllamaMessagesFromMessage(message));

        ollamaRequest.messages = messages;
        ollamaRequest.model = request.model;
        ollamaRequest.tools = BuildOllamaToolDefinitions(tools);
        return ollamaRequest;
    }

    private IEnumerable<OllamaMessage> OllamaMessagesFromMessage(Message message)
    {
        List<OllamaMessage> messages = new();
        
        if(message.SystemPrompt is not null)
        {
            messages.Add(new()
            {
                role = "system",
                content = message.SystemPrompt
            });
        }

        if(message.UserPrompt is not null)
        {
            messages.Add(new()
            {
                role = "user",
                content = message.UserPrompt
            });
        }

        if(message.ResponseReceivedAt is not null)
        {
            messages.Add(new()
            {
                role = "assistant",
                content = message.ModelResponse
            });
        }

        if(message.ToolCalls is not null)
        {
            messages.Add(new()
            {
                role = "assistant",
                content = "",
                tool_calls = BuildToolCallsFromMessage(message)
            });
        }

        if(message.ToolResponses is not null)
            messages.AddRange(BuildToolResultsFromMessage(message));

        return messages;
    }

    private IEnumerable<OllamaToolDefinition> BuildOllamaToolDefinitions(IEnumerable<ToolDefinition> tools) =>
        tools.Select(t => new OllamaToolDefinition
        {
            type = t.Type,
            function = new OllamaToolFunction
            {
                name = t.Name,
                description = t.Description,
                parameters = new OllamaToolFunctionParameters
                {
                    type = t.Parameters.Type,
                    properties = t.Parameters.Properties.ToDictionary(
                        p => p.Name,
                        p => new OllamaToolFunctionParameter
                        {
                            type = p.Type,
                            description = p.Description
                        }
                    ),
                    required = t.Parameters.Properties.Where(pp => pp.Required).Select(pp => pp.Name)
                }
            }
        });

    private IEnumerable<OllamaToolCall> BuildToolCallsFromMessage(Message message) =>
        JsonSerializer.Deserialize<IEnumerable<OllamaToolCall>>(message.ToolCalls);

    private IEnumerable<OllamaMessage> BuildToolResultsFromMessage(Message message)
    {
        List<OllamaMessage> results = new();

        if(message.ToolResponses is null)
            return results;

        var responses = JsonSerializer.Deserialize<Dictionary<string, string>>(message.ToolResponses);

        foreach (var item in responses)
        {
            results.Add(new()
            {
                role = "tool",
                tool_name = item.Key,
                content = item.Value
            });
        }

        return results;
    }
}