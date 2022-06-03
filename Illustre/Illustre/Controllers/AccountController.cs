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

        return Redirect(IndexRedirect);
    }

    [HttpGet]
    public async Task<IActionResult> Account()
    {
        if (Request.Cookies.TryGetValue(SessionCookie, out var cookie))
        {
            var account = await _accountService.TryGetAccount(cookie);
            return View(account);
        }

        return Redirect(IndexRedirect);
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

        return Redirect(IndexRedirect);
    }

    [HttpGet]
    public async Task<IActionResult> ManageEditors(ManageAccountsRequest request)
    {
        return await Execute(
            new Role[] { Role.SuperAdmin },
            request,
            async (request) =>
            {
                var dto = request as ManageAccountsRequest;
                dto!.AccountsData = await _accountService
                    .GetEditors(dto.Skip, dto.SearchPattern);
                return View(dto);
            });
    }

    [HttpGet]
    public async Task<IActionResult> ManageUsers(ManageAccountsRequest request)
    {
        return await Execute(
            new Role[] { Role.SuperAdmin },
            request,
            async (request) =>
            {
                var dto = request as ManageAccountsRequest;
                dto!.AccountsData = await _accountService
                    .GetUsers(dto.Skip, dto.SearchPattern);
                return View(dto);
            });
    }

    [HttpPost]
    public async Task<IActionResult> AddAccount(AddAccountRequest request)
    {
        return await Execute(
            new Role[] { Role.SuperAdmin },
            request,
            async (request) =>
            {
                var dto = request as AddAccountRequest;
                var result = false.ToString().ToLower();
                var action = dto.Role == Role.Editor ?
                             "ManageEditors" :
                             "ManageUsers";

                if (ModelState.IsValid)
                {
                    result = (await _accountService.TryAddAccount(dto!))
                        .ToString()
                        .ToLower();
                }
                
                return Redirect($"/Account/{action}?isFirstAttempt={result}");
            });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateAccountById(ManageAccountModel model)
    {
        return await Execute(
            new Role[] { Role.SuperAdmin },
            model,
            async (model) =>
            {
                var dto = model as ManageAccountModel;
                var result = false.ToString().ToLower();
                var action = dto.Role == Role.Editor ?
                             "ManageEditors" :
                             "ManageUsers";

                if (ModelState.IsValid)
                {
                    result = (await _accountService.TryUpdateAccountById(dto!))
                    .ToString()
                    .ToLower();
                }
                
                return Redirect($"/Account/{action}?isFirstAttempt={result}");
            });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
