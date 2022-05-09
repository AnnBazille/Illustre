using Microsoft.AspNetCore.Mvc;
using Service;

namespace Illustre.Controllers;

public class MainController : CommonController
{
    public MainController(AccountService accountService) : base(accountService) { }

    [HttpGet]
    public async Task<IActionResult> SuperAdminMenu()
    {
        var redirect = await TryRedirect();

        if (redirect == null)
        {
            return Redirect("/Account/Index");
        }

        if ((redirect as RedirectResult)!.Url != "/Main/SuperAdminMenu")
        {
            return redirect;
        }

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> EditorMenu()
    {
        var redirect = await TryRedirect();

        if (redirect == null)
        {
            return Redirect("/Account/Index");
        }

        if ((redirect as RedirectResult)!.Url != "/Main/EditorMenu")
        {
            return redirect;
        }

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> UserMenu()
    {
        var redirect = await TryRedirect();

        if (redirect == null)
        {
            return Redirect("/Account/Index");
        }

        if ((redirect as RedirectResult)!.Url != "/Main/UserMenu")
        {
            return redirect;
        }

        return View();
    }
}
