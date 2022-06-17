using Data.Contracts.Media;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Data.Repositories;

public class MediaRepository : BaseRepository
{
    private readonly BlobStorageHelper _blobStorageHelper;

    public MediaRepository(
        DatabaseContext databaseContext,
        BlobStorageHelper blobStorageHelper) : base(databaseContext)
    {
        _blobStorageHelper = blobStorageHelper;
    }

    public async Task<ManageTagsModel> GetTags(int skip, string? search)
    {
        var result = new ManageTagsModel();

        Expression<Func<Tag, bool>> predicate = x => string.IsNullOrEmpty(search) ||
                                                     x.Title.Contains(search);

        result.Total = await DatabaseContext.Tags
            .AsNoTracking()
            .CountAsync(predicate);

        Expression<Func<Tag, bool>> predicateSelected = x => (string.IsNullOrEmpty(search) ||
                                                             x.Title.Contains(search)) &&
                                                             x.IsActive;

        result.Selected = await DatabaseContext.Tags
            .AsNoTracking()
            .CountAsync(predicateSelected);

        result.Models = await DatabaseContext.Tags
            .AsNoTracking()
            .Where(predicate)
            .OrderByDescending(x => x.IsActive)
            .ThenByDescending(x => x.Id)
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

        Expression<Func<Image, bool>> imagePredicateSelected = x => (imageIds.Contains(x.Id) ||
                                                                    string.IsNullOrEmpty(search) ||
                                                                    x.Title.Contains(search)) &&
                                                                    x.IsActive;

        result.Selected = await DatabaseContext.Images
            .AsNoTracking()
            .CountAsync(imagePredicateSelected);

        result.Models = await DatabaseContext.Images
            .AsNoTracking()
            .Where(imagePredicate)
            .OrderByDescending(x => x.IsActive)
            .ThenByDescending(x => x.Id)
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
                item.Image = _blobStorageHelper
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

            await _blobStorageHelper
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

        var activeIds = await DatabaseContext.ImageProperties
            .AsNoTracking()
            .Where(x => x.ImageId == imageId &&
                        x.IsActive)
            .OrderByDescending(x => x.Id)
            .Select(x => x.TagId)
            .ToListAsync();

        result.Selected = activeIds.Count;

        Expression<Func<Tag, bool>> predicate = x => (string.IsNullOrEmpty(search) ||
                                                     x.Title.Contains(search)) &&
                                                     !activeIds.Contains(x.Id);

        var allIds = await DatabaseContext.Tags
            .AsNoTracking()
            .Where(predicate)
            .OrderByDescending(x => x.IsActive)
            .ThenByDescending(x => x.Id)
            .Select(x => x.Id)
            .ToListAsync();

        allIds.InsertRange(0, activeIds);

        result.Total = allIds.Count();

        var resultIds = allIds
            .Skip(skip)
            .Take(ConstantsHelper.PageSize);

        var models = await DatabaseContext.Tags
            .AsNoTracking()
            .Where(x => resultIds.Contains(x.Id))
            .Select(x => new EditTagModel()
            {
                Id = x.Id,
                Title = x.Title,
                ImageId = imageId,
            })
            .ToDictionaryAsync(
            key => key.Id,
            value => value);

        foreach (var item in activeIds)
        {
            if (models.ContainsKey(item))
            {
                models[item].IsActive = true;
            } 
        }

        var resultModels = new List<ManageTagModel>();

        foreach (var item in resultIds)
        {
            if (models.ContainsKey(item))
            {
                resultModels.Add(models[item]);
            }
        }

        result.Models = resultModels;

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

    public async Task<ManageImagesModel> GetEditableImages(
        int skip,
        string? search,
        int tagId)
    {
        var result = new ManageImagesModel();

        var activeIds = await DatabaseContext.ImageProperties
            .AsNoTracking()
            .Where(x => x.TagId == tagId &&
                        x.IsActive)
            .OrderByDescending(x => x.Id)
            .Select(x => x.ImageId)
            .ToListAsync();

        result.Selected = activeIds.Count;

        Expression<Func<Tag, bool>> tagPredicate = x =>
            !string.IsNullOrEmpty(search) &&
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

        Expression<Func<Image, bool>> imagePredicate = x =>
            (imageIds.Contains(x.Id) ||
            string.IsNullOrEmpty(search) ||
            x.Title.Contains(search)) &&
            !activeIds.Contains(x.Id);

        var allIds = await DatabaseContext.Images
            .AsNoTracking()
            .Where(imagePredicate)
            .OrderByDescending(x => x.IsActive)
            .ThenByDescending(x => x.Id)
            .Select(x => x.Id)
            .ToListAsync();

        allIds.InsertRange(0, activeIds);

        result.Total = allIds.Count();

        var resultIds = allIds
            .Skip(skip)
            .Take(ConstantsHelper.PageSize);

        var models = await DatabaseContext.Images
            .AsNoTracking()
            .Where(x => resultIds.Contains(x.Id))
            .Select(x => new EditImageModel()
            {
                Id = x.Id,
                Title = x.Title,
            })
            .ToDictionaryAsync(
            key => key.Id,
            value => value);

        foreach (var item in activeIds)
        {
            if (models.ContainsKey(item))
            {
                models[item].IsActive = true;    
            }
        }

        foreach (var item in models)
        {
            item.Value.TagId = tagId;
            try
            {
                item.Value.Image = _blobStorageHelper
                .DownloadImage(item.Value.Id.ToString());
            }
            catch { }
        }

        var resultModels = new List<ManageImageModel>();

        foreach (var item in resultIds)
        {
            if (models.ContainsKey(item))
            {
                resultModels.Add(models[item]);
            }
        }

        result.Models = resultModels;

        return result;
    }

    public async Task<ShowImageModel> GetNextImage(int userId)
    {
        var reactions = await DatabaseContext.Reactions
            .Where(x => x.AccountId == userId && x.IsActive)
            .OrderBy(x => Guid.NewGuid())
            .Take(10)
            .Select(x => new
            {
                ImageId = x.ImageId,
                IsLiked = x.IsLiked,
            })
            .ToListAsync();

        if (reactions.Count == 0)
        {
            return await CreateShowImageModel();
        }

        var imageIds = reactions.Select(x => x.ImageId).ToList();

        var imageProperties = await DatabaseContext.ImageProperties
            .Where(x => x.IsActive &&
                        imageIds.Contains(x.ImageId))
            .Select(x => new
            {
                ImageId = x.ImageId,
                TagId = x.TagId
            })
            .ToListAsync();

        var resultTagIds = imageProperties
            .Select(x => x.TagId)
            .Distinct()
            .ToList();

        var ratings = new Dictionary<int, int>();

        foreach (var tag in resultTagIds)
        {
            var images = imageProperties
                .Where(x => x.TagId == tag)
                .Select(x => x.ImageId)
                .ToList();

            var tagReactions = reactions
                .Where(x => images.Contains(x.ImageId))
                .ToList();

            var likes = tagReactions
                .Count(x => x.IsLiked);

            var dislikes = tagReactions.Count() - likes;

            var tagRating = likes - dislikes;

            ratings.TryAdd(tagRating, tag);
        }

        var topRating = ratings.Keys.Max();

        var topTag = ratings[topRating];

        var usedImageProperties = DatabaseContext.ImageProperties
            .Join(DatabaseContext.Reactions,
            property => property.ImageId,
            reaction => reaction.ImageId,
            (property, reaction) => new
            {
                ImagePropertyId = property.Id,
                IsActiveProperty = property.IsActive,
                IsActiveReaction = reaction.IsActive,
                TagId = property.TagId,
                UserId = reaction.AccountId,
            })
            .Where(x => x.IsActiveReaction &&
                        x.IsActiveProperty &&
                        x.UserId == userId &&
                        x.TagId == topTag)
            .Select(x => x.ImagePropertyId);

        var imageId = await DatabaseContext.ImageProperties
            .Where(x => x.IsActive &&
                        x.TagId == topTag &&
                        !usedImageProperties.Contains(x.Id))
            .OrderByDescending(x => x.Id)
            .Select(x => x.ImageId)
            .FirstOrDefaultAsync();

        return await CreateShowImageModel(imageId);
    }

    public async Task SetReaction(int userId, int imageId, bool isLiked)
    {
        var reaction = await DatabaseContext.Reactions
            .FirstOrDefaultAsync(x => x.ImageId == imageId &&
                                      x.AccountId == userId);

        var isNew = false;

        if (reaction is null)
        {
            isNew = true;
            reaction = new Reaction()
            {
                AccountId = userId,
                ImageId = imageId,
            };
        }

        reaction.IsLiked = isLiked;

        try
        {
            if (isNew)
            {
                await DatabaseContext.Reactions.AddAsync(reaction);
            }

            await DatabaseContext.SaveChangesAsync();
        }
        catch { }
    }

    private async Task<ShowImageModel> CreateShowImageModel(int? imageId = null)
    {
        ShowImageModel result;

        if (imageId is null)
        {
            result = await DatabaseContext.Images
                .Where(x => x.IsActive)
                .OrderBy(x => Guid.NewGuid())
                .Select(x => new ShowImageModel()
                {
                    ImageId = x.Id,
                    Title = x.Title,
                })
                .FirstAsync();
        }
        else
        {
            result = await DatabaseContext.Images
                .Where(x => x.Id == imageId)
                .Select(x => new ShowImageModel()
                {
                    ImageId = x.Id,
                    Title = x.Title,
                })
                .FirstAsync();
        }

        var tagIds = await DatabaseContext.ImageProperties
                .Where(x => x.IsActive &&
                            x.ImageId == result!.ImageId)
                .Select(x => x.TagId)
                .ToListAsync();

        result!.Tags = await DatabaseContext.Tags
            .Where(x => tagIds.Contains(x.Id))
            .Select(x => new ShowTagModel()
            {
                Id = x.Id,
                Title = x.Title,
            })
            .ToListAsync();

        result.Image = _blobStorageHelper
            .DownloadImage(result.ImageId.ToString());

        return result;
    }
}
