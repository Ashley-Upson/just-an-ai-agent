using JustAnAiAgent.AI;
using Microsoft.AspNetCore.Mvc;

namespace JustAnAiAgent.Controllers.Api.AI;

[Route("api/agent")]
public class ChatController(ILogger<HomeController> logger, AIChatClient chatClient)
    : Controller
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] string message, [FromQuery] string model) =>
        Ok(await chatClient.SendMessageAsync(message, model));
}