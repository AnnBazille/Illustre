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
                    return RedirectPermanent("/Main/SuperAdminMenu");
                }
            case Role.Editor:
                {
                    return RedirectPermanent("/Main/EditorMenu");
                }
            case Role.User:
                {
                    return RedirectPermanent("/Main/UserMenu");
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
}
