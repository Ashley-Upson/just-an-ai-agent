using cCoder.Security.Objects;
using cCoder.Security.Services.Orchestration.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JustAnAiAgent.Controllers;

public class HomeController(
    ILogger<HomeController> logger,
    ISSOAuthInfoOrchestrationService authInfoOrchestrationService)
    : Controller
{
    private readonly ISSOAuthInfoOrchestrationService authInfoOrchestrationService = authInfoOrchestrationService;

    [HttpGet("")]
    public IActionResult Index()
    {
        ISSOAuthInfo user = authInfoOrchestrationService.GetSSOAuthInfo();

        if (user.SSOUserId == "Guest")
            return Redirect("Account/Login");

        return View();
    }
}