using cCoder.Security.Api.Interfaces;
using cCoder.Security.Objects;
using cCoder.Security.Objects.DTOs;
using cCoder.Security.Objects.Entities;
using cCoder.Security.Services.Orchestration.Interfaces;
using JustAnAiAgent.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace JustAnAiAgent.Controllers;

[Route("account")]
public class AccountController(
    ILogger<AccountController> logger,
    IAccountManager accountManager,
    ISSOAuthInfoOrchestrationService authInfoOrchestrationService)
    : Controller
{
    [HttpGet("Login")]
    public IActionResult Login([FromQuery] string success, [FromQuery] string error)
    {
        string userId = CurrentUserId();

        if (userId != "Guest")
            return Redirect("/");

        ViewBag.Success = success;
        ViewBag.Error = error;

        return View();
    }

    [HttpGet("Register")]
    public IActionResult Register([FromQuery] string[] errors)
    {
        string userId = CurrentUserId();

        if (userId != "Guest")
            return Redirect("/");

        ViewBag.Errors = errors;

        return View();
    }

    [HttpPost("Login")]
    public async ValueTask<IActionResult> Login([FromForm] Auth auth)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await accountManager.LoginAsync(auth.User, auth.Pass);
        }
        catch (Exception ex)
        {
            return Redirect($"/Account/Login?error={ex.Message}");
        }

        return Redirect("/");
    }

    [HttpPost("Register")]
    public async ValueTask<IActionResult> Register([FromForm] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        List<string> errors = [];

        if (request.Password != null && request.Password != request.ConfirmPassword)
            errors.Add("Passwords do not match.");

        if (request.Password.Length < 12)
            errors.Add("Password must be at least 12 characters long.");

        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.DisplayName))
            errors.Add("Email and DisplayName must be provided.");

        if (errors.Count > 0)
            return Redirect("/Account/Register?" + string.Join('&', errors.Select(e => "errors=" + e)));

        RegisterUser form = new();
        form.Email = request.Email;
        form.DisplayName = request.DisplayName;
        form.Password = request.Password;

        try
        {
            (SSOUser user, string token) = await accountManager.RegisterAsync(form);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception.Message, exception);

            return Redirect($"/Account/Register?errors={exception.Message}");
        }

        return Redirect("/Account/Login?success=Registration successful.");
    }

    private string CurrentUserId()
    {
        ISSOAuthInfo user = authInfoOrchestrationService.GetSSOAuthInfo();
        return user.SSOUserId;
    }
}
