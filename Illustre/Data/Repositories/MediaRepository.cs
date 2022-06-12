using Data.Contracts.Media;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Data.Repositories;

public class MediaRepository : BaseRepository
{
    private readonly BlogStorageHelper _blogStorageHelper;

    public MediaRepository(
        DatabaseContext databaseContext,
        BlogStorageHelper blogStorageHelper) : base(databaseContext)
    {
        _blogStorageHelper = blogStorageHelper;
    }

    public async Task<ManageTagsModel> GetTags(int skip, string? search)
    {
        var result = new ManageTagsModel();

        Expression<Func<Tag, bool>> predicate = x => string.IsNullOrEmpty(search) ||
                                                     x.Title.Contains(search);

        result.Total = await DatabaseContext.Tags
            .AsNoTracking()
            .CountAsync(predicate);
        result.Models = await DatabaseContext.Tags
            .AsNoTracking()
            .Where(predicate)
            .OrderBy(x => x.Id)
            .Skip(skip)
            .Take(ConstantsHelper.PageSize)
            .Select(x => new ManageTagModel()
            {
                Id = x.Id,
                IsActive = x.IsActive,
                Title = x.Title,
            })
            .ToListAsync();

        return result;
    }

    public async Task<bool> TryAddTag(AddTagRequest request)
    {
        var tag = new Tag()
        {
            Title = request.Title,
        };

        try
        {
            await DatabaseContext.Tags.AddAsync(tag);
            await DatabaseContext.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> TryUpdateTagById(ManageTagModel model)
    {
        var tag = await DatabaseContext.Tags
            .FirstOrDefaultAsync(x => x.Id == model.Id);

        if (tag == null)
        {
            return false;
        }

        tag.IsActive = model.IsActive;

        if (model.Title is not null)
        {
            tag.Title = model.Title;
        }

        try
        {
            await DatabaseContext.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<ManageImagesModel> GetImages(int skip, string? search)
    {
        var result = new ManageImagesModel();

        Expression<Func<Tag, bool>> tagPredicate = x => !string.IsNullOrEmpty(search) &&
                                                        x.Title.Contains(search);

        var tagIds = await DatabaseContext.Tags
            .AsNoTracking()
            .Where(tagPredicate)
            .Select(x => x.Id)
            .ToListAsync();

        var imageIds = await DatabaseContext.ImageProperties
            .AsNoTracking()
            .Where(x => tagIds.Contains(x.TagId))
            .Select(x => x.ImageId)
            .Distinct()
            .ToListAsync();

        Expression<Func<Image, bool>> imagePredicate = x => imageIds.Contains(x.Id) ||
                                                            string.IsNullOrEmpty(search) ||
                                                            x.Title.Contains(search);

        result.Total = await DatabaseContext.Images
            .AsNoTracking()
            .CountAsync(imagePredicate);

        result.Models = await DatabaseContext.Images
            .AsNoTracking()
            .Where(imagePredicate)
            .OrderBy(x => x.Id)
            .Skip(skip)
            .Take(ConstantsHelper.PageSize)
            .Select(x => new ManageImageModel()
            {
                Id = x.Id,
                IsActive = x.IsActive,
                Title = x.Title,
            })
            .ToListAsync();

        foreach (var item in result.Models)
        {
            try
            {
                item.Image = await _blogStorageHelper
                    .DownloadImage(item.Id.ToString());
            }
            catch { }
        }

        return result;
    }

    public async Task<bool> TryAddImage(AddImageRequest request)
    {
        var image = new Image()
        {
            Title = request.Title,
        };

        var result = true;

        try
        {
            await DatabaseContext.Images.AddAsync(image);
            await DatabaseContext.SaveChangesAsync();

            await _blogStorageHelper
                .UploadImage(image.Id.ToString(), request.File);
        }
        catch
        {
            result = false;
        }

        return result;
    }

    public async Task<bool> TryUpdateImageById(ManageImageModel model)
    {
        var image = await DatabaseContext.Images
            .FirstOrDefaultAsync(x => x.Id == model.Id);

        if (image == null)
        {
            return false;
        }

        image.IsActive = model.IsActive;

        if (model.Title is not null)
        {
            image.Title = model.Title;
        }

        try
        {
            await DatabaseContext.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<ManageTagsModel> GetEditableTags(int skip, string? search, int imageId)
    {
        var result = new ManageTagsModel();

        Expression<Func<Tag, bool>> predicate = x => string.IsNullOrEmpty(search) ||
                                                     x.Title.Contains(search);

        result.Total = await DatabaseContext.Tags
            .AsNoTracking()
            .CountAsync(predicate);
        result.Models = await DatabaseContext.Tags
            .AsNoTracking()
            .Where(predicate)
            .OrderBy(x => x.Id)
            .Skip(skip)
            .Take(ConstantsHelper.PageSize)
            .Select(x => new EditTagModel()
            {
                Id = x.Id,
                Title = x.Title,
                IsActive = false,
            })
            .ToListAsync();

        var tagIds = result.Models
            .Select(x => x.Id);

        var imageProperties = await DatabaseContext.ImageProperties
            .AsNoTracking()
            .Where(x => tagIds.Contains(x.TagId) &&
                        x.ImageId == imageId &&
                        x.IsActive)
            .ToDictionaryAsync(
            key => key.TagId,
            value => value.ImageId);

        foreach (var item in result.Models)
        {
            var model = item as EditTagModel;
            model!.ImageId = imageId;

            if (imageProperties.ContainsKey(model!.Id))
            {
                model.IsActive = true;
            }
        }

        return result;
    }

    public async Task<bool> TryAddImageProperty(int tagId, int imageId, bool isActive)
    {
        var imageProperty = await DatabaseContext.ImageProperties
            .FirstOrDefaultAsync(x => x.TagId == tagId &&
                                      x.ImageId == imageId);

        var isNew = false;

        if (imageProperty == null)
        {
            isNew = true;
            imageProperty = new ImageProperty()
            {
                TagId = tagId,
                ImageId = imageId,
            };
        }

        imageProperty.IsActive = isActive;

        try
        {
            if (isNew)
            {
                await DatabaseContext.ImageProperties.AddAsync(imageProperty);
            }
            
            await DatabaseContext.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<ManageImagesModel> GetEditableImages(int skip, string? search, int tagId)
    {
        var result = new ManageImagesModel();

        Expression<Func<Tag, bool>> tagPredicate = x => !string.IsNullOrEmpty(search) &&
                                                        x.Title.Contains(search);

        var tagIds = await DatabaseContext.Tags
            .AsNoTracking()
            .Where(tagPredicate)
            .Select(x => x.Id)
            .ToListAsync();

        var imageIds = await DatabaseContext.ImageProperties
            .AsNoTracking()
            .Where(x => tagIds.Contains(x.TagId))
            .Select(x => x.ImageId)
            .Distinct()
            .ToListAsync();

        Expression<Func<Image, bool>> imagePredicate = x => imageIds.Contains(x.Id) ||
                                                            string.IsNullOrEmpty(search) ||
                                                            x.Title.Contains(search);

        result.Total = await DatabaseContext.Images
            .AsNoTracking()
            .CountAsync(imagePredicate);
        result.Models = await DatabaseContext.Images
            .AsNoTracking()
            .Where(imagePredicate)
            .OrderBy(x => x.Id)
            .Skip(skip)
            .Take(ConstantsHelper.PageSize)
            .Select(x => new EditImageModel()
            {
                Id = x.Id,
                Title = x.Title,
                IsActive = false,
            })
            .ToListAsync();

        var resultImageIds = result.Models
            .Select(x => x.Id)
            .ToList();

        var imageProperties = await DatabaseContext.ImageProperties
            .AsNoTracking()
            .Where(x => resultImageIds.Contains(x.ImageId) &&
                        x.TagId == tagId &&
                        x.IsActive)
            .ToDictionaryAsync(
            key => key.ImageId,
            value => value.TagId);

        foreach (var item in result.Models)
        {
            var model = item as EditImageModel;
            model!.TagId = tagId;

            if (imageProperties.ContainsKey(model!.Id))
            {
                model.IsActive = true;
            }

            try
            {
                item.Image = await _blogStorageHelper
                    .DownloadImage(item.Id.ToString());
            }
            catch { }
        }

        return result;
    }
}
