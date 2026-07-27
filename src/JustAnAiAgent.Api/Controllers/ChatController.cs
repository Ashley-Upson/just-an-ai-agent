using System.Text.Json;
using JustAnAiAgent.Objects.Entities;
using JustAnAiAgent.Services.Orchestration.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JustAnAiAgent.Api.Controllers;


[Route("/Api/Chat")]
public class ChatController(IOllamaOrchestrationService ollamaService) : ControllerBase
{
    [HttpPost("ConversationWithNewMessage/{id}")]
    public async Task<IActionResult> Post([FromRoute] Guid id, [FromBody] Message message)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        return Ok(await ollamaService.AddMessageAndSendToModel(id, message));
    }

    [HttpPost("ConversationWithNewMessageStream/{id}")]
    public async Task<IActionResult> PostStream([FromRoute] Guid id, [FromBody] Message message, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        Response.ContentType = "application/x-ndjson";

        await foreach (Message streamMessage in ollamaService.AddMessageAndSendToModelStream(id, message, cancellationToken))
        {
            string payload = JsonSerializer.Serialize(streamMessage);
            await Response.WriteAsync($"{payload}\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }

        return new EmptyResult();
    }
}
