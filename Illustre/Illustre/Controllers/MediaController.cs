using Data.Contracts.Media;
using Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace Illustre.Controllers;

public class MediaController : CommonController
{
    private readonly MediaService _mediaService;
    public MediaController(
        AccountService accountService,
        MediaService mediaService)
        : base(accountService)
    {
        _mediaService = mediaService;
    }

    [HttpGet]
    public async Task<IActionResult> ManageTags(ManageTagsRequest request)
    {
        if (Request.Cookies.TryGetValue(SessionCookie, out var cookie))
        {
            var role = await _accountService.TryGetRoleBySessionGuid(cookie);

            if (role is not null &&
               (role == Role.SuperAdmin ||
                role == Role.Editor))
            {
                request.TagsData = await _mediaService
                    .GetTags(request.Skip, request.SearchPattern);
                return View(request);
            }
        }

        return Redirect("/Account/Index");
    }

    [HttpPost]
    public async Task<IActionResult> AddTag(AddTagRequest request)
    {
        if (Request.Cookies.TryGetValue(SessionCookie, out var cookie))
        {
            var role = await _accountService.TryGetRoleBySessionGuid(cookie);

            if (role is not null &&
               (role == Role.SuperAdmin ||
                role == Role.Editor))
            {
                var result = (await _mediaService.TryAddTag(request))
                    .ToString()
                    .ToLower();
                return Redirect($"/Media/ManageTags?isFirstAttempt={result}");
            }
        }

        return Redirect("/Account/Index");
    }

    [HttpPost]
    public async Task<IActionResult> UpdateTagById(ManageTagModel model)
    {
        if (Request.Cookies.TryGetValue(SessionCookie, out var cookie))
        {
            var role = await _accountService.TryGetRoleBySessionGuid(cookie);

            if (role is not null &&
               (role == Role.SuperAdmin ||
                role == Role.Editor))
            {
                var result = (await _mediaService.TryUpdateTagById(model))
                    .ToString()
                    .ToLower();
                return Redirect($"/Media/ManageTags?isFirstAttempt={result}");
            }
        }

        return Redirect("/Account/Index");
    }
}
