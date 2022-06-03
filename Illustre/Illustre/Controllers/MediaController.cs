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
        return await Execute(
            new Role[] { Role.SuperAdmin, Role.Editor },
            request,
            async (request) =>
            {
                var dto = request as ManageTagsRequest;
                dto!.TagsData = await _mediaService
                    .GetTags(dto.Skip, dto.SearchPattern);
                return View(dto);
            });
    }

    [HttpPost]
    public async Task<IActionResult> AddTag(AddTagRequest request)
    {
        return await Execute(
            new Role[] { Role.SuperAdmin, Role.Editor },
            request,
            async (request) =>
            {
                var dto = request as AddTagRequest;
                var result = false.ToString().ToLower();

                if (ModelState.IsValid)
                {
                    result = (await _mediaService.TryAddTag(dto!))
                        .ToString()
                        .ToLower();
                }
                
                return Redirect(GetManageTagsRedirect(result));
            });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateTagById(ManageTagModel model)
    {
        return await Execute(
            new Role[] { Role.SuperAdmin, Role.Editor },
            model,
            async (model) =>
            {
                var dto = model as ManageTagModel;
                var result = false.ToString().ToLower();

                if (ModelState.IsValid)
                {
                    result = (await _mediaService.TryUpdateTagById(dto!))
                        .ToString()
                        .ToLower();
                }
                    
                return Redirect(GetManageTagsRedirect(result));
            });
    }

    private string GetManageTagsRedirect(string isFirstAttempt)
    {
        return $"/Media/ManageTags?isFirstAttempt={isFirstAttempt}";
    }
}
