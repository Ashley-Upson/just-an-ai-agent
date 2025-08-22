using JustAnAiAgent.Objects.Entities;
using JustAnAiAgent.Services.Foundation.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace JustAnAiAgent.Api.Controllers;

[Route("/Api/AgenticProject")]
public class AgenticProjectController(IAgenticProjectService projectService) : ControllerBase
{
    [EnableQuery]
    [HttpGet]
    public IActionResult Get() =>
        Ok(projectService.GetAll());

    [EnableQuery]
    [HttpGet("{id}")]
    public async Task<IActionResult> Get([FromRoute] Guid id)
    {
        var project = await projectService.GetAsync(id);

        if (project == null)
            return NotFound();

        return Ok(project);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] AgenticProject project)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var added = await projectService.AddAsync(project);
        return Ok(added);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put([FromRoute] Guid id, [FromBody] AgenticProject project)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await projectService.UpdateAsync(id, project);
        if (updated == null)
            return NotFound();

        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await projectService.DeleteAsync(id);

        return NoContent();
    }
}
