using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using JustAnAiAgent.MCP.MCP;
using JustAnAiAgent.Objects.Entities;
using JustAnAiAgent.Objects.Ollama;
using JustAnAiAgent.Objects.Providers;

namespace JustAnAiAgent.Providers.Ollama;

public class OllamaClient
{
    private string ApiUrl { get; set; }

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

    public async IAsyncEnumerable<OllamaResponse> SendChatMessageWithToolsStreamAsync(
        ProviderChatRequest request,
        IEnumerable<ToolDefinition> tools,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        OllamaRequest ollamaRequest = BuildChatRequestWithTools(request, tools, stream: true);
        string payload = JsonSerializer.Serialize(ollamaRequest);

        using HttpRequestMessage httpRequest = new(HttpMethod.Post, "chat")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        using HttpResponseMessage httpResponse = await ApiClient.SendAsync(
            httpRequest,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        httpResponse.EnsureSuccessStatusCode();

        await using Stream stream = await httpResponse.Content.ReadAsStreamAsync(cancellationToken);
        using StreamReader reader = new(stream);

        JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true
        };

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            string line = await reader.ReadLineAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(line))
                continue;

            OllamaResponse chunk = JsonSerializer.Deserialize<OllamaResponse>(line, options);

            if (chunk is not null)
                yield return chunk;
        }
    }

    private OllamaRequest BuildBasicChatRequest(ProviderChatRequest request)
    {
        OllamaRequest ollamaRequest = new();
        ollamaRequest.stream = false;
        ollamaRequest.options = new()
        {
            { "num_ctx", 20000 },
            { "num_thread", 20 }
        };
        List<OllamaMessage> messages = new();

        foreach (var message in request.messages)
            messages.AddRange(OllamaMessagesFromMessage(message));

        ollamaRequest.messages = messages;
        ollamaRequest.model = request.model;
        return ollamaRequest;
    }

    private OllamaRequest BuildChatRequestWithTools(ProviderChatRequest request, IEnumerable<ToolDefinition> tools, bool stream = false)
    {
        OllamaRequest ollamaRequest = new();
        ollamaRequest.stream = stream;
        ollamaRequest.options = new()
        {
            { "num_ctx", 65565 },
            { "num_thread", 20 }
        };
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

        switch (message.Type)
        {
            case "system":
                messages.Add(new()
                {
                    role = "system",
                    content = message.Content
                });
                break;

            case "user":
                messages.Add(new()
                {
                    role = "user",
                    content = message.Content
                });
                break;

            case "response":
                messages.Add(new()
                {
                    role = "assistant",
                    content = message.Content
                });
                break;

            case "tool-calls":
                messages.Add(new()
                {
                    role = "assistant",
                    content = "",
                    tool_calls = BuildToolCallsFromMessage(message)
                });
                break;

            case "tool-results":
                messages.AddRange(BuildToolResultsFromMessage(message));
                break;

            default:
                break;
        }

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

    private IEnumerable<OllamaToolCall> BuildToolCallsFromMessage(Message message)
    {
        if (message.Type != "tool-calls")
            return [];

        return JsonSerializer.Deserialize<IEnumerable<OllamaToolCall>>(message.Content);
    }

    private IEnumerable<OllamaMessage> BuildToolResultsFromMessage(Message message)
    {
        List<OllamaMessage> results = new();

        if (message.Type != "tool-results")
            return results;

        var responses = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(message.Content);

        foreach (var item in responses)
        {
            results.Add(new()
            {
                role = "tool",
                tool_name = item.Key,
                content = item.Value.ValueKind == JsonValueKind.String
                    ? item.Value.GetString()
                    : item.Value.GetRawText()
            });
        }

        return results;
    }
}
