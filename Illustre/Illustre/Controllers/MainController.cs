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

        return redirect ?? View();
    }

    [HttpGet]
    public async Task<IActionResult> EditorMenu()
    {
        var redirect = await TryRedirect();

        return redirect ?? View();
    }

    [HttpGet]
    public async Task<IActionResult> UserMenu()
    {
        var redirect = await TryRedirect();

        return redirect ?? View();
    }
}
