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

                var redirect = dto!.Action + $"?isFirstAttempt={result}";

                return Redirect(redirect);
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

    [HttpGet]
    public async Task<IActionResult> ManageImages(ManageImagesRequest request)
    {
        return await Execute(
            new Role[] { Role.SuperAdmin, Role.Editor },
            request,
            async (request) =>
            {
                var dto = request as ManageImagesRequest;
                dto!.ImagesData = await _mediaService
                    .GetImages(dto.Skip, dto.SearchPattern);
                return View(dto);
            });
    }

    [HttpPost]
    public async Task<IActionResult> AddImage(AddImageRequest request)
    {
        return await Execute(
            new Role[] { Role.SuperAdmin, Role.Editor },
            request,
            async (request) =>
            {
                var dto = request as AddImageRequest;
                var result = false.ToString().ToLower();

                if (ModelState.IsValid)
                {
                    result = (await _mediaService.TryAddImage(dto!))
                        .ToString()
                        .ToLower();
                }

                return Redirect(GetManageImagesRedirect(result));
            });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateImageById(ManageImageModel model)
    {
        return await Execute(
            new Role[] { Role.SuperAdmin, Role.Editor },
            model,
            async (model) =>
            {
                var dto = model as ManageImageModel;
                var result = false.ToString().ToLower();

                if (ModelState.IsValid)
                {
                    result = (await _mediaService.TryUpdateImageById(dto!))
                        .ToString()
                        .ToLower();
                }

                return Redirect(GetManageImagesRedirect(result));
            });
    }

    [HttpGet]
    public async Task<IActionResult> EditTags(EditTagsRequest request)
    {
        return await Execute(
            new Role[] { Role.SuperAdmin, Role.Editor },
            request,
            async (request) =>
            {
                var dto = request as EditTagsRequest;
                dto!.TagsData = await _mediaService
                .GetEditableTags(dto.Skip, dto.SearchPattern, dto.ImageId);
                return View(dto);
            });
    }

    private string GetManageTagsRedirect(string isFirstAttempt)
    {
        return $"/Media/ManageTags?isFirstAttempt={isFirstAttempt}";
    }

    private string GetManageImagesRedirect(string isFirstAttempt)
    {
        return $"/Media/ManageImages?isFirstAttempt={isFirstAttempt}";
    }
}
