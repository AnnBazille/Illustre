using Microsoft.AspNetCore.Mvc;

namespace Illustre.Controllers;

public class MainController : Controller
{
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
