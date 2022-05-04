using Data.Contracts.Accounts;
using Data.Entities;
using Illustre.Models;
using Microsoft.AspNetCore.Mvc;
using Service;
using System.Diagnostics;

namespace Illustre.Controllers;

public class AccountController : Controller
{
    private const string SessionCookie = "session-id";
    private const string UsernameCookie = "username";
    private readonly ILogger<AccountController> _logger;
    private readonly AccountService _accountService;

    public AccountController(
        ILogger<AccountController> logger,
        AccountService accountService)
    {
        _logger = logger;
        _accountService = accountService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(SignInRequest request)
    {
        if (Request.Cookies.TryGetValue(SessionCookie, out var cookie))
        {
            var role = await _accountService.TryGetRoleBySessionGuid(cookie!);
            if (role != null)
            {
                var result = RedirectAuthenticated(role!.Value);
                if (result != null)
                {
                    return result;
                }
            }
        }

        return View(request);
    }

    [HttpPost]
    public async Task<IActionResult> SignIn(SignInRequest request)
    {
        if (ModelState.IsValid)
        {
            var response = await _accountService.TrySignIn(request);

            if (response != null)
            {
                var options = new CookieOptions()
                {
                    Expires = response.Expires,
                    Domain = Request.Host.Host,
                };

                Response.Cookies.Append(
                    SessionCookie,
                    response.SessionGuid,
                    options);

                Response.Cookies.Append(
                    UsernameCookie,
                    response.Username,
                    options);

                var result = RedirectAuthenticated(response.Role);
                if (result != null)
                {
                    return result;
                }
            }
        }

        return RedirectPermanent("/Account/Index?isFirstAttempt=false");
    }

    public async Task<IActionResult> SignUp()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private RedirectResult? RedirectAuthenticated(Role role)
    {
        switch (role)
        {
            case Role.SuperAdmin:
                {
                    return RedirectPermanent("/Main/SuperAdmin");
                }
            case Role.Editor:
                {
                    return RedirectPermanent("/Main/Editor");
                }
            case Role.User:
                {
                    return RedirectPermanent("/Main/User");
                }
        }

        return null;
    }
}
