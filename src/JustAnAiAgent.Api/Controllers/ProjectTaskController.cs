using JustAnAiAgent.Objects.Entities;
using JustAnAiAgent.Services.Foundation.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace JustAnAiAgent.Api.Controllers;

[Route("/Api/ProjectTask")]
public class ProjectTaskController(IProjectTaskService taskService) : ControllerBase
{
    [EnableQuery]
    [HttpGet]
    public IActionResult Get() =>
        Ok(taskService.GetAll());

    [EnableQuery]
    [HttpGet("{id}")]
    public async Task<IActionResult> Get([FromRoute] Guid id)
    {
        var task = await taskService.GetAsync(id);

        if (task == null)
            return NotFound();

        return Ok(task);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ProjectTask task)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var added = await taskService.AddAsync(task);
        return Ok(added);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put([FromRoute] Guid id, [FromBody] ProjectTask task)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await taskService.UpdateAsync(id, task);
        if (updated == null)
            return NotFound();

        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await taskService.DeleteAsync(id);

        return NoContent();
    }
}
