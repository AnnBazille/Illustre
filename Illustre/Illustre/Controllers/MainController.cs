using Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace Illustre.Controllers;

public class MainController : CommonController
{
    public MainController(
        AccountService accountService)
        : base(accountService) { }

    [HttpGet]
    public async Task<IActionResult> SuperAdminMenu()
    {
        return await Execute(
            new Role[] { Role.SuperAdmin },
            NoParameters,
            async (NoParameters) =>
            {
                return View();
            });
    }

    [HttpGet]
    public async Task<IActionResult> EditorMenu()
    {
        return await Execute(
            new Role[] { Role.Editor },
            NoParameters,
            async (NoParameters) =>
            {
                return View();
            });
    }

    [HttpGet]
    public async Task<IActionResult> UserMenu()
    {
        return await Execute(
            new Role[] { Role.User },
            NoParameters,
            async (NoParameters) =>
            {
                return Redirect("/Media/Image");
            });
    }
}
