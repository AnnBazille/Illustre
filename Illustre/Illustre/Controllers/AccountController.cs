using Data.Contracts.Accounts;
using Data.Entities;
using Illustre.Models;
using Microsoft.AspNetCore.Mvc;
using Service;
using System.Diagnostics;

namespace Illustre.Controllers;

public class AccountController : CommonController
{
    public AccountController(AccountService accountService) : base(accountService) { }

    [HttpGet]
    public async Task<IActionResult> Index(SignInRequest request)
    {
        var redirect = await TryRedirect();

        return redirect ?? View(request);
    }

    [HttpPost]
    public async Task<IActionResult> SignIn(SignInRequest request)
    {
        var redirect = await TryRedirect();
        if (redirect != null)
        {
            return redirect;
        }

        if (ModelState.IsValid)
        {
            var response = await _accountService.TrySignIn(request);

            if (response != null)
            {
                SetCookies(response);

                var result = RedirectAuthenticated(response.Role);
                if (result != null)
                {
                    return result;
                }
            }
        }

        return RedirectPermanent("/Account/Index?isFirstAttempt=false");
    }

    [HttpGet]
    public async Task<IActionResult> SignUp(SignUpRequest request)
    {
        var redirect = await TryRedirect();

        return redirect ?? View(request);
    }

    [HttpPost]
    public async Task<IActionResult> Register(SignUpRequest request)
    {
        var redirect = await TryRedirect();
        if (redirect != null)
        {
            return redirect;
        }

        if (ModelState.IsValid)
        {
            var response = await _accountService.TrySignUp(request);

            if (response != null)
            {
                SetCookies(response);

                var result = RedirectAuthenticated(response.Role);
                if (result != null)
                {
                    return result;
                }
            }
        }

        return RedirectPermanent("/Account/SignUp?isFirstAttempt=false");
    }

    [HttpGet]
    public async Task<IActionResult> LogOut()
    {
        var redirect = await TryRedirect();

        if (redirect != null)
        {
            RemoveCookies();
        }

        return RedirectPermanent("/Account/Index");
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
}
