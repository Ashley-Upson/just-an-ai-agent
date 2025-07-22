using System.Text.Json;
using System.Xml.Linq;
using JustAnAiAgent.AI.Models;

namespace JustAnAiAgent.AI;

public class AIChatClient(string hostingServerUrl, string fallbackModel)
{
    readonly List<MessageData> history = [];

    private readonly HttpClient client = new() 
    { 
        BaseAddress = new Uri(hostingServerUrl), 
        Timeout = TimeSpan.FromMinutes(10)
    };

    public async Task<dynamic> SendMessageAsync(string message, string model)
    {
        history.Add(new MessageData { Role = "user", Content = message });

        var requestBody = new
        {
            model = model,
            messages = history,
            options = new 
            { 
                temperature = 1.0,
                max_tokens = 2048
            }
        };

        var response = await client
            .PostAsJsonAsync("/api/chat", requestBody);

        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        var responseJson = $"[{responseString.Trim().Replace("\n", ",")}]";

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var parsedResponse = JsonSerializer
            .Deserialize<ChatResponse[]>(responseJson, options);

        history.AddRange(parsedResponse.Select(i => i.Message));

        var responseParts = parsedResponse
            .Select(i => i.Message.Content);

        var thoughtAndReply = string.Join("", responseParts)
            .Split("</think>");

        return thoughtAndReply.Length > 1 
            ? new 
            { 
                thought = thoughtAndReply[0].TrimStart("<think>".ToArray()).Trim(), 
                reply = thoughtAndReply[1].Trim()
            }
            : new
            {
                thought = string.Empty,
                reply = thoughtAndReply[1].Trim()
            };
    }
}