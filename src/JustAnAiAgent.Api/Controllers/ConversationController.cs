using JustAnAiAgent.Objects.Entities;
using JustAnAiAgent.Services.Processing.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace JustAnAiAgent.Api.Controllers;

[Route("/Api/Conversation")]
public class ConversationController(IConversationProcessingService conversationService) : ControllerBase
{
    [EnableQuery]
    [HttpGet]
    public IActionResult Get() =>
        Ok(conversationService.GetAll());

    [EnableQuery]
    [HttpGet("{id}")]
    public async Task<IActionResult> Get([FromRoute] Guid id)
    {
        var conversation = await conversationService.GetWithMessagesAsync(id);

        if (conversation == null)
            return NotFound();

        return Ok(conversation);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Conversation conversation)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var added = await conversationService.AddAsync(conversation);
        return Ok(added);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put([FromRoute] Guid id, [FromBody] Conversation conversation)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await conversationService.UpdateAsync(id, conversation);
        if (updated == null)
            return NotFound();

        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await conversationService.DeleteAsync(id);

        return NoContent();
    }
}
