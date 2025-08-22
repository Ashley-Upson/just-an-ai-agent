using JustAnAiAgent.Objects.Entities;
using JustAnAiAgent.Services.Foundation.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace JustAnAiAgent.Api.Controllers;

[Route("/Api/UserConversation")]
public class UserConversationController(IUserConversationService userConversationService) : ControllerBase
{
    [EnableQuery]
    [HttpGet]
    public IActionResult Get() =>
        Ok(userConversationService.GetAll());

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] UserConversation userConversation)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var added = await userConversationService.AddAsync(userConversation);
        return Ok(added);
    }

    [HttpDelete("{userId}/{conversationId}")]
    public async Task<IActionResult> Delete([FromRoute] string userId, [FromRoute] Guid conversationId)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await userConversationService.DeleteAsync(userId, conversationId);

        return NoContent();
    }
}