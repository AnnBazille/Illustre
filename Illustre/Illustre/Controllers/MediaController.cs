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

                if (dto.ImageId is not null)
                {
                    redirect += $"&imageId={dto.ImageId}";
                }

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
                .GetEditableTags(dto.Skip, dto.SearchPattern, dto.ImageId ?? 0);
                return View(dto);
            });
    }

    [HttpPost]
    public async Task<IActionResult> EditTagById(EditTagModel model)
    {
        return await Execute(
            new Role[] { Role.SuperAdmin, Role.Editor },
            model,
            async (model) =>
            {
                var dto = model as EditTagModel;
                var result = false.ToString().ToLower();

                if (ModelState.IsValid)
                {
                    result = (await _mediaService.TryEditTagById(dto!))
                        .ToString()
                        .ToLower();
                }

                return Redirect(GetEditTagsRedirect(result, dto!.ImageId));
            });
    }

    [HttpGet]
    public async Task<IActionResult> EditImages(EditImagesRequest request)
    {
        return await Execute(
            new Role[] { Role.SuperAdmin, Role.Editor },
            request,
            async (request) =>
            {
                var dto = request as EditImagesRequest;
                dto!.ImagesData = await _mediaService
                .GetEditableImages(dto.Skip, dto.SearchPattern, dto.TagId ?? 0);
                return View(dto);
            });
    }

    [HttpPost]
    public async Task<IActionResult> EditImageById(EditImageModel model)
    {
        return await Execute(
            new Role[] { Role.SuperAdmin, Role.Editor },
            model,
            async (model) =>
            {
                var dto = model as EditImageModel;
                var result = false.ToString().ToLower();

                if (ModelState.IsValid)
                {
                    result = (await _mediaService.TryEditImageById(dto!))
                        .ToString()
                        .ToLower();
                }

                return Redirect(GetEditImagesRedirect(result, dto!.TagId));
            });
    }

    [HttpGet]
    public async Task<IActionResult> Image()
    {
        return await Execute(
            new Role[] { Role.User },
            NoParameters,
            async (NoParameters) =>
            {
                var array = NoParameters as object[];
                var cookie = array![CookieIndex] as string;
                var userId = await _accountService
                    .TryGetAccountIdBySessionGuid(cookie!);

                if (userId is not null)
                {
                    var image = await _mediaService.GetNextImage(userId!.Value);
                    return View(image);
                }
                else
                {
                    return RedirectToAction("Account", "Account");
                }
            },
            true);
    }

    [HttpGet]
    public async Task<IActionResult> GetImage(int imageId)
    {
        return await Execute(
            new Role[] { Role.User },
            imageId,
            async (model) =>
            {
                var array = model as object[];
                var cookie = array![CookieIndex] as string;
                var imageId = (int)array[DtoIndex];
                var userId = await _accountService
                    .TryGetAccountIdBySessionGuid(cookie!);

                var result = await _mediaService.GetImage(
                    userId!.Value,
                    imageId);

                return View("Image", result);
            },
            true);
    }

    [HttpGet]
    public async Task<IActionResult> SetReaction(SetReactionModel model)
    {
        return await Execute(
            new Role[] { Role.User },
            model,
            async (model) =>
            {
                var array = model as object[];
                var cookie = array![CookieIndex] as string;
                var dto = array[DtoIndex] as SetReactionModel;
                var userId = await _accountService
                    .TryGetAccountIdBySessionGuid(cookie!);

                if (userId is not null)
                {
                    dto!.UserId = userId!.Value;

                    await _mediaService.SetReaction(dto);

                    return RedirectToAction("Image", "Media");
                }
                else
                {
                    return RedirectToAction("Account", "Account");
                }
            },
            true);
    }

    [HttpGet]
    public async Task<IActionResult> Search(SearchModel model)
    {
        return await Execute(
            new Role[] { Role.User },
            model,
            async (model) =>
            {
                var array = model as object[];
                var cookie = array![CookieIndex] as string;
                var dto = array[DtoIndex] as SearchModel;
                var userId = await _accountService
                    .TryGetAccountIdBySessionGuid(cookie!);

                if (userId is not null)
                {
                    dto!.UserId = userId!.Value;

                    var result = await _mediaService.GetImagePreviews(dto);

                    result.SearchModel = dto;

                    return View(result);
                }
                else
                {
                    return RedirectToAction("Account", "Account");
                }
            },
            true);
    }

    private string GetManageTagsRedirect(string isFirstAttempt)
    {
        return $"/Media/ManageTags?isFirstAttempt={isFirstAttempt}";
    }

    private string GetManageImagesRedirect(string isFirstAttempt)
    {
        return $"/Media/ManageImages?isFirstAttempt={isFirstAttempt}";
    }

    private string GetEditTagsRedirect(string isFirstAttempt, int imageId)
    {
        return $"/Media/EditTags?isFirstAttempt={isFirstAttempt}&imageId="
               + imageId;
    }

    private string GetEditImagesRedirect(string isFirstAttempt, int tagId)
    {
        return $"/Media/EditImages?isFirstAttempt={isFirstAttempt}&tagId="
               + tagId;
    }
}
