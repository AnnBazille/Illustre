using Data.Contracts.Accounts;
using Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace Illustre.Controllers;

public abstract class CommonController : Controller
{
    public const string SessionCookie = "session-id";

    public const string UsernameCookie = "username";

    public const string RoleCookie = "role";

    public const string SuperAdminMenuRedirect = "/Main/SuperAdminMenu";

    public const string EditorMenuRedirect = "/Main/EditorMenu";

    public const string UserMenuRedirect = "/Main/UserMenu";

    public const string IndexRedirect = "/Account/Index";

    public const string NoParameters = "";

    protected readonly AccountService _accountService;

    public CommonController(AccountService accountService)
    {
        _accountService = accountService;
    }

    public async Task<IActionResult?> TryRedirect()
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

        return null;
    }

    public RedirectResult? RedirectAuthenticated(Role role)
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

    public void SetCookies(SignInResponse response)
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

        Response.Cookies.Append(
            RoleCookie,
            response.Role.ToString(),
            options);
    }

    public void RemoveCookies()
    {
        Response.Cookies.Delete(SessionCookie);
        Response.Cookies.Delete(UsernameCookie);
        Response.Cookies.Delete(RoleCookie);
    }

    public async Task<IActionResult> Execute(
        Role[] allowedRoles,
        object dto,
        Func<object, Task<IActionResult>> action)
    {
        if (Request.Cookies.TryGetValue(SessionCookie, out var cookie))
        {
            var role = await _accountService.TryGetRoleBySessionGuid(cookie);

            if (role is not null &&
                allowedRoles.Contains(role.Value))
            {
                return await action(dto);
            }
        }

        return Redirect(IndexRedirect);
    }
}
