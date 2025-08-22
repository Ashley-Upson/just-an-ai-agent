using JustAnAiAgent.Objects.Entities;
using JustAnAiAgent.Providers;
using JustAnAiAgent.Services.Foundation;
using JustAnAiAgent.Services.Orchestration.Interfaces;
using JustAnAiAjent.Objects.Providers;
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
}
