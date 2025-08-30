using JustAnAiAgent.MCP.Interfaces;
using JustAnAiAgent.MCP.MCP;
using JustAnAiAgent.Objects.Entities;
using JustAnAiAgent.Services.Processing.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace JustAnAiAgent.Api.Controllers;

[Route("/Api/MCP")]
public class McpController(IEnumerable<IMcpTool> tools) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTools()
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        List<ToolDefinition> definitions = new();

        foreach (var tool in tools)
            definitions.Add(tool.GetToolDefinition());

        return Ok(definitions);
    }

    [HttpPost("Execute/{name}")]
    public async Task<IActionResult> Execute([FromBody] IEnumerable<ToolParameterInput> parameters)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        //IMcpTool tool = tools.FirstOrDefault(t => t.Name );

        return Ok();
    }
}