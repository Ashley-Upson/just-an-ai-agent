using System.Threading.Tasks;
using JustAnAiAgent.Objects.Entities;
using JustAnAiAgent.Services.Foundation.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace JustAnAiAgent.Api.Controllers;

[Route("/Api/Models")]
public class ModelsController(IEnumerable<ILLMProviderService> providers) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        List<string> models = new();

        foreach (var provider in providers)
            models.AddRange(await provider.GetAvailableModelsAsync());

        return Ok(models);
    }
}
