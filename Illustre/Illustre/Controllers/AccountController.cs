using Data;
using Data.Contracts.Accounts;
using Data.Entities;
using Illustre.Models;
using Microsoft.AspNetCore.Mvc;
using Service;
using System.Diagnostics;

namespace Illustre.Controllers;

public class AccountController : CommonController
{
    public AccountController(AccountService accountService)
        : base(accountService) { }

    [HttpGet]
    public async Task<IActionResult> Index(SignInRequest request)
    {
        return await ExecuteRedirect(
            null,
            async (request) =>
            {
                var dto = request as SignInRequest;
                return View(dto);
            },
            request);
    }

    [HttpPost]
    public async Task<IActionResult> SignIn(SignInRequest request)
    {
        return await ExecuteRedirect(
            null,
            async (request) =>
            {
                var dto = request as SignInRequest;

                if (ModelState.IsValid)
                {
                    var response = await _accountService.TrySignIn(dto!);

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
            },
            request);
    }

    [HttpGet]
    public async Task<IActionResult> SignUp(SignUpRequest request)
    {
        return await ExecuteRedirect(
            null,
            async (request) =>
            {
                var dto = request as SignUpRequest;
                return View(dto);
            },
            request);
    }

    [HttpPost]
    public async Task<IActionResult> Register(SignUpRequest request)
    {
        return await ExecuteRedirect(
            null,
            async (request) =>
            {
                var dto = request as SignUpRequest;
                if (ModelState.IsValid)
                {
                    var response = await _accountService.TrySignUp(dto!);

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
            },
            request);
    }

    [HttpGet]
    public async Task<IActionResult> LogOut()
    {
        return await ExecuteRedirect(
            () =>
            {
                RemoveCookies();
                return Redirect(IndexRedirect);
            },
            async (NoParameters) =>
            {
                return Redirect(IndexRedirect);
            },
            NoParameters);
    }

    [HttpGet]
    public async Task<IActionResult> Account()
    {
        return await Execute(
            Array.Empty<Role>(),
            NoParameters,
            async (NoParameters) =>
            {
                var array = NoParameters as object[];
                var cookie = array![CookieIndex] as string;

                var account = await _accountService.TryGetAccount(cookie!);
                return View(account);
            },
            true);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateAccount(UpdateAccountRequest request)
    {
        return await Execute(
            Array.Empty<Role>(),
            request,
            async (request) =>
            {
                var array = request as object[];
                var dto = array![DtoIndex] as UpdateAccountRequest;
                var cookie = array[CookieIndex] as string;

                var result = await _accountService
                    .TryUpdateAccount(cookie!, dto!);

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
                        ConstantsHelper.UsernameCookie,
                        result.Username,
                        options);

                    return View("Account", result);
                }
            },
            true);
    }

    [HttpGet]
    public async Task<IActionResult> ManageEditors
        (ManageAccountsRequest request)
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
                var action = dto!.Role == Role.Editor ?
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
                var action = dto!.Role == Role.Editor ?
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
}
