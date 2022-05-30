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

        return Redirect("/Account/Index?isFirstAttempt=false");
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

        return Redirect("/Account/SignUp?isFirstAttempt=false");
    }

    [HttpGet]
    public async Task<IActionResult> LogOut()
    {
        var redirect = await TryRedirect();

        if (redirect != null)
        {
            RemoveCookies();
        }

        return Redirect("/Account/Index");
    }

    [HttpGet]
    public async Task<IActionResult> Account()
    {
        if (Request.Cookies.TryGetValue(SessionCookie, out var cookie))
        {
            var account = await _accountService.TryGetAccount(cookie);
            return View(account);
        }

        return Redirect("/Account/Index");
    }

    [HttpPost]
    public async Task<IActionResult> UpdateAccount(UpdateAccountRequest request)
    {
        if (Request.Cookies.TryGetValue(SessionCookie, out var cookie))
        {
            var result = await _accountService.TryUpdateAccount(cookie, request);

            if (result == null)
            {
                return Redirect("/Account/Account?isFirstAttempt=false");
            }
            else
            {
                var options = new CookieOptions()
                {
                    Expires = DateTime.UtcNow.AddDays(1),
                    Domain = Request.Host.Host,
                };

                Response.Cookies.Append(
                    UsernameCookie,
                    result.Username,
                    options);

                return View("Account", result);
            }
        }

        return Redirect("/Account/Index");
    }

    [HttpGet]
    public async Task<IActionResult> ManageEditors(ManageEditorsRequest request)
    {
        if (Request.Cookies.TryGetValue(SessionCookie, out var cookie))
        {
            var role = await _accountService.TryGetRoleBySessionGuid(cookie);

            if (role is not null &&
                role == Role.SuperAdmin)
            {
                request.AccountsData = await _accountService
                    .GetEditors(request.Skip, request.SearchPattern);
                return View(request);
            }
        }

        return Redirect("/Account/Index");
    }

    [HttpPost]
    public async Task<IActionResult> AddAccount(AddAccountRequest request)
    {
        if (Request.Cookies.TryGetValue(SessionCookie, out var cookie))
        {
            var role = await _accountService.TryGetRoleBySessionGuid(cookie);

            if (role is not null &&
                role == Role.SuperAdmin)
            {
                var result = (await _accountService.TryAddAccount(request))
                    .ToString()
                    .ToLower();
                var action = request.Role == Role.Editor ?
                             "ManageEditors" :
                             "ManageUsers";
                return Redirect($"/Account/{action}?isFirstAttempt={result}");
            }
        }

        return Redirect("/Account/Index");
    }

    [HttpPost]
    public async Task<IActionResult> UpdateAccountById(ManageAccountModel model)
    {
        if (Request.Cookies.TryGetValue(SessionCookie, out var cookie))
        {
            var role = await _accountService.TryGetRoleBySessionGuid(cookie);

            if (role is not null &&
                role == Role.SuperAdmin)
            {
                var result = (await _accountService.TryUpdateAccountById(model))
                    .ToString()
                    .ToLower();
                var action = model.Role == Role.Editor ?
                             "ManageEditors" :
                             "ManageUsers";
                return Redirect($"/Account/{action}?isFirstAttempt={result}");
            }
        }

        return Redirect("/Account/Index");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
