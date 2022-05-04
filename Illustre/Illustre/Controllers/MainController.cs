using Microsoft.AspNetCore.Mvc;
using Service;

namespace Illustre.Controllers;

public class MainController : CommonController
{
    public MainController(AccountService accountService) : base(accountService) { }

    [HttpGet]
    public async Task<IActionResult> SuperAdmin()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Editor()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> User()
    {
        return View();
    }
}
