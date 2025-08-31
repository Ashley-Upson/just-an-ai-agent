using System.ComponentModel.DataAnnotations;
using JustAnAiAgent.MCP.Interfaces;
using JustAnAiAgent.MCP.MCP;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> Execute([FromRoute] string name, [FromBody] IEnumerable<ToolParameterInput> parameters)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        IMcpTool tool = tools.FirstOrDefault(t => t.Name == name);

        if (tool == null)
            throw new ValidationException($"Tool {name} does not exist.");

        string result = await tool.Execute(parameters);

        return Ok(result);
    }
}