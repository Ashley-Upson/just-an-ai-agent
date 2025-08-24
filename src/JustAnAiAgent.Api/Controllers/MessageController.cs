using JustAnAiAgent.Objects.Entities;
using JustAnAiAgent.Services.Processing.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace JustAnAiAgent.Api.Controllers;

[Route("/Api/Message")]
public class MessageController(IMessageProcessingService messageService) : ControllerBase
{
    [EnableQuery]
    [HttpGet]
    public IActionResult Get() =>
        Ok(messageService.GetAll());

    [EnableQuery]
    [HttpGet("{id}")]
    public async Task<IActionResult> Get([FromRoute] Guid id)
    {
        var conversation = await messageService.GetAsync(id);

        if (conversation == null)
            return NotFound();

        return Ok(conversation);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Message message)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var added = await messageService.AddAsync(message);
        return Ok(added);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put([FromRoute] Guid id, [FromBody] Message message)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await messageService.UpdateAsync(id, message);
        if (updated == null)
            return NotFound();

        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await messageService.DeleteAsync(id);

        return NoContent();
    }
}
