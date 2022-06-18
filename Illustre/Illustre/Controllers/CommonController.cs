using Data;
using Data.Contracts.Accounts;
using Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace Illustre.Controllers;

public abstract class CommonController : Controller
{
    public const string SuperAdminMenuRedirect = "/Main/SuperAdminMenu";

    public const string EditorMenuRedirect = "/Main/EditorMenu";

    public const string UserMenuRedirect = "/Main/UserMenu";

    public const string IndexRedirect = "/Account/Index";

    public const string NoParameters = "";

    public const int DtoIndex = 0;

    public const int CookieIndex = 1;

    protected readonly AccountService _accountService;

    public CommonController(AccountService accountService)
    {
        _accountService = accountService;
    }

    protected async Task<IActionResult?> TryRedirect()
    {
        if (Request.Cookies
                .TryGetValue(ConstantsHelper.SessionCookie, out var cookie))
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

        return null;
    }

    protected RedirectResult? RedirectAuthenticated(Role role)
    {
        switch (role)
        {
            case Role.SuperAdmin:
                {
                    return Redirect(SuperAdminMenuRedirect);
                }
            case Role.Editor:
                {
                    return Redirect(EditorMenuRedirect);
                }
            case Role.User:
                {
                    return Redirect(UserMenuRedirect);
                }
        }

        return null;
    }

    protected void SetCookies(SignInResponse response)
    {
        var options = new CookieOptions()
        {
            Expires = response.Expires,
            Domain = Request.Host.Host,
        };

        Response.Cookies.Append(
            ConstantsHelper.SessionCookie,
            response.SessionGuid,
            options);

        Response.Cookies.Append(
            ConstantsHelper.UsernameCookie,
            response.Username,
            options);

        Response.Cookies.Append(
            ConstantsHelper.RoleCookie,
            response.Role.ToString(),
            options);
    }

    protected void RemoveCookies()
    {
        Response.Cookies.Delete(ConstantsHelper.SessionCookie);
        Response.Cookies.Delete(ConstantsHelper.UsernameCookie);
        Response.Cookies.Delete(ConstantsHelper.RoleCookie);
    }

    protected async Task<IActionResult> Execute(
        Role[] allowedRoles,
        object dto,
        Func<object, Task<IActionResult>> action,
        bool injectCookie = false)
    {
        if (Request.Cookies
                .TryGetValue(ConstantsHelper.SessionCookie, out var cookie) &&
            await CheckRoles(allowedRoles, cookie!))
        {
            var parameter = dto;

            if (injectCookie)
            {
                var array = new object[] { dto, cookie! };
                parameter = array;
            }

            return await action(parameter);
        }

        return Redirect(IndexRedirect);
    }

    protected async Task<IActionResult> ExecuteRedirect(
        Func<IActionResult>? redirectNotNullAction,
        Func<object, Task<IActionResult>> redirectNullAction,
        object redirectNullParameter)
    {
        var redirect = await TryRedirect();

        if (redirect != null)
        {
            return redirectNotNullAction is not null ?
                   redirectNotNullAction() :
                   redirect;
        }

        return await redirectNullAction(redirectNullParameter);
    }

    private async Task<bool> CheckRoles(
        Role[] allowedRoles,
        string cookie)
    {
        var role = await _accountService.TryGetRoleBySessionGuid(cookie);

        return role is not null &&
              (allowedRoles.Count() == 0 ||
               allowedRoles.Contains(role.Value));
    }
}
