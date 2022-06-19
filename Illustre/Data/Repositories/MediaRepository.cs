using Data.Contracts.Media;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Data.Repositories;

public class MediaRepository : BaseRepository
{
    private readonly BlobStorageHelper _blobStorageHelper;

    public class TagRating
    {
        public int TagId { get; set; }

        public int Rating { get; set; }
    }

    public MediaRepository(
        DatabaseContext databaseContext,
        BlobStorageHelper blobStorageHelper) : base(databaseContext)
    {
        _blobStorageHelper = blobStorageHelper;
    }

    public async Task<ManageTagsModel> GetTags(int skip, string? search)
    {
        var result = new ManageTagsModel();

        Expression<Func<Tag, bool>> predicate = x =>
            string.IsNullOrEmpty(search) ||
            x.Title.Contains(search);

        result.Total = await DatabaseContext.Tags
            .AsNoTracking()
            .CountAsync(predicate);

        Expression<Func<Tag, bool>> predicateSelected = x =>
            (string.IsNullOrEmpty(search) ||
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
            imageIds.Contains(x.Id) ||
            string.IsNullOrEmpty(search) ||
            x.Title.Contains(search);

        result.Total = await DatabaseContext.Images
            .AsNoTracking()
            .CountAsync(imagePredicate);

        Expression<Func<Image, bool>> imagePredicateSelected = x =>
            (imageIds.Contains(x.Id) ||
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

    public async Task<ManageTagsModel> GetEditableTags(
        int skip,
        string? search,
        int imageId)
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

        Expression<Func<Tag, bool>> predicate = x =>
            (string.IsNullOrEmpty(search) ||
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

    public async Task<bool> TryAddImageProperty(
        int tagId,
        int imageId,
        bool isActive)
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
        const int maxReactionsCount = 100;

        var randomReactions = await DatabaseContext.Reactions
            .AsNoTracking()
            .Where(x => x.IsActive &&
                        x.AccountId == userId)
            .OrderBy(x => Guid.NewGuid())
            .Take(maxReactionsCount)
            .ToListAsync();

        var likedImages = randomReactions
            .Where(x => x.IsLiked &&
                        x.IsActive)
            .Select(x => x.ImageId)
            .ToList();

        var dislikedImages = randomReactions
            .Where(x => !x.IsLiked &&
                        x.IsActive)
            .Select(x => x.ImageId)
            .ToList();

        var likedTags = await DatabaseContext.ImageProperties
            .AsNoTracking()
            .Join(
            DatabaseContext.Tags,
            property => property.TagId,
            tag => tag.Id,
            (property, tag) => new
            {
                TagId = tag.Id,
                IsActiveTag = tag.IsActive,
                IsActiveProperty = property.IsActive,
                ImageId = property.ImageId,
            })
            .Where(x => x.IsActiveTag &&
                        x.IsActiveProperty &&
                        likedImages.Contains(x.ImageId))
            .GroupBy(x => x.TagId)
            .Select(x => new TagRating()
            {
                TagId = x.Key,
                Rating = x.Count(),
            })
            .ToListAsync();

        var dislikedTags = await DatabaseContext.ImageProperties
            .AsNoTracking()
            .Join(
            DatabaseContext.Tags,
            property => property.TagId,
            tag => tag.Id,
            (property, tag) => new
            {
                TagId = tag.Id,
                IsActiveTag = tag.IsActive,
                IsActiveProperty = property.IsActive,
                ImageId = property.ImageId,
            })
            .Where(x => x.IsActiveTag &&
                        x.IsActiveProperty &&
                        dislikedImages.Contains(x.ImageId))
            .GroupBy(x => x.TagId)
            .Select(x => new TagRating()
            {
                TagId = x.Key,
                Rating = x.Count(),
            })
            .ToListAsync();

        /*var dislikedTags = await DatabaseContext.ImageProperties
            .Where(x => x.IsActive &&
                        dislikedImages.Contains(x.ImageId))
            .GroupBy(x => x.TagId)
            .Select(x => new TagRating()
            {
                TagId = x.Key,
                Rating = x.Count(),
            })
            .ToListAsync();*/

        var ratings = new List<TagRating>();

        foreach (var tag in likedTags)
        {
            var dislikes = dislikedTags
                .FirstOrDefault(x => x.TagId == tag.TagId)?.Rating ?? 0;

            tag.Rating -= dislikes;

            ratings.Add(tag);
        }

        ratings.RemoveAll(x => x.Rating < 0);

        var result = ratings
            .OrderByDescending(x => x.Rating)
            .ThenBy(x => Guid.NewGuid());

        foreach (var tag in result)
        {
            var seenImages = DatabaseContext.ImageProperties
                .AsNoTracking()
                .Join(
                DatabaseContext.Reactions.AsNoTracking(),
                property => property.ImageId,
                reaction => reaction.ImageId,
                (property, reaction) => new
                {
                    IsActiveProperty = property.IsActive,
                    IsActiveReaction = reaction.IsActive,
                    UserId = reaction.AccountId,
                    TagId = property.TagId,
                    ImageId = property.ImageId,
                })
                .Where(x => x.IsActiveReaction &&
                            x.IsActiveProperty &&
                            x.UserId == userId &&
                            x.TagId == tag.TagId)
                .Select(x => x.ImageId)
                .Join(
                DatabaseContext.Images.AsNoTracking(),
                x => x,
                image => image.Id,
                (x, image) => new
                {
                    ImageId = image.Id,
                    IsActive = image.IsActive,
                })
                .Where(x => x.IsActive)
                .Select(x => x.ImageId);

            var notSeenImages = DatabaseContext.Images
                .Where(x => !seenImages.Contains(x.Id) &&
                            x.IsActive);

            var count = await notSeenImages.CountAsync();

            if (count != 0)
            {
                var imageId = await notSeenImages
                    .OrderBy(x => Guid.NewGuid())
                    .Select(x => x.Id)
                    .FirstAsync();

                return await CreateShowImageModel(userId, imageId);
            }
        }

        return await CreateShowImageModel(userId);
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
                IsLiked = isLiked,
            };
        }
        else if (reaction.IsLiked != isLiked)
        {
            reaction.IsLiked = isLiked;
            reaction.IsActive = true;
        }
        else
        {
            reaction.IsActive = !reaction.IsActive;
        }

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

    public async Task<PreviewImagesModel> GetImagePreviews(
        int userId,
        string searchPattern,
        int? tagId,
        int skip,
        bool? isLiked)
    {
        var result = new PreviewImagesModel();

        if (tagId is null &&
            isLiked is null)
        {
            Expression<Func<Tag, bool>> tagPredicate = x =>
                x.IsActive &&
                !string.IsNullOrEmpty(searchPattern) &&
                x.Title.Contains(searchPattern);

            var tagIds = await DatabaseContext.Tags
                .AsNoTracking()
                .Where(tagPredicate)
                .Select(x => x.Id)
                .ToListAsync();

            var imageIds = await DatabaseContext.ImageProperties
                .AsNoTracking()
                .Where(x => tagIds.Contains(x.TagId) &&
                            x.IsActive)
                .Select(x => x.ImageId)
                .Distinct()
                .ToListAsync();

            Expression<Func<Image, bool>> imagePredicate = x =>
                x.IsActive &&
                (imageIds.Contains(x.Id) ||
                (!string.IsNullOrEmpty(searchPattern) &&
                x.Title.Contains(searchPattern)));

            result.Total = await DatabaseContext.Images
                .AsNoTracking()
                .CountAsync(imagePredicate);

            var images = await DatabaseContext.Images
               .AsNoTracking()
               .Where(imagePredicate)
               .OrderByDescending(x => x.Id)
               .Skip(skip)
               .Select(x => new PreviewImageModel()
               {
                   ImageId = x.Id,
                   Title = x.Title,
               })
               .Take(ConstantsHelper.PageSize)
               .ToListAsync();

            foreach (var image in images)
            {
                try
                {
                    image.Image = _blobStorageHelper
                        .DownloadImage(image.ImageId.ToString());
                }
                catch { }
            }

            result.Images = images;
        }
        else if (tagId is not null &&
                 isLiked is null)
        {
            Expression<Func<Tag, bool>> tagPredicate = x =>
                x.IsActive &&
                (x.Id == tagId ||
                (!string.IsNullOrEmpty(searchPattern) &&
                x.Title.Contains(searchPattern)));

            var tagIds = await DatabaseContext.Tags
                .AsNoTracking()
                .Where(tagPredicate)
                .Select(x => x.Id)
                .ToListAsync();

            var imageIds = await DatabaseContext.ImageProperties
                .AsNoTracking()
                .Where(x => tagIds.Contains(x.TagId) &&
                            x.IsActive)
                .Select(x => x.ImageId)
                .Distinct()
                .ToListAsync();

            Expression<Func<Image, bool>> imagePredicate = x =>
                x.IsActive &&
                (imageIds.Contains(x.Id) ||
                (!string.IsNullOrEmpty(searchPattern) &&
                x.Title.Contains(searchPattern)));

            result.Total = await DatabaseContext.Images
                .AsNoTracking()
                .CountAsync(imagePredicate);

            var images = await DatabaseContext.Images
               .AsNoTracking()
               .Where(imagePredicate)
               .OrderByDescending(x => x.Id)
               .Skip(skip)
               .Select(x => new PreviewImageModel()
               {
                   ImageId = x.Id,
                   Title = x.Title,
               })
               .Take(ConstantsHelper.PageSize)
               .ToListAsync();

            foreach (var image in images)
            {
                try
                {
                    image.Image = _blobStorageHelper
                        .DownloadImage(image.ImageId.ToString());
                }
                catch { }
            }

            result.Images = images;
        }
        else if (tagId is null &&
                 isLiked is not null)
        {
            var reactedImagesIds = DatabaseContext.Reactions
                .AsNoTracking()
                .Where(x => x.IsActive &&
                            x.IsLiked == isLiked &&
                            x.AccountId == userId)
                .Select(x => x.ImageId)
                .Join(
                DatabaseContext.Images,
                x => x,
                image => image.Id,
                (x, image) => new
                {
                    ImageId = image.Id,
                    IsActive = image.IsActive,
                })
                .Where(x => x.IsActive)
                .Select(x => x.ImageId);

            var tagIds = DatabaseContext.ImageProperties
                .AsNoTracking()
                .Where(x => x.IsActive &&
                            reactedImagesIds.Contains(x.ImageId))
                .Select(x => x.TagId)
                .Join(
                DatabaseContext.Tags,
                x => x,
                tag => tag.Id,
                (x, tag) => new
                {
                    TagId = tag.Id,
                    Title = tag.Title,
                    IsActive = tag.IsActive,
                })
                .Where(x => x.IsActive &&
                            !string.IsNullOrEmpty(searchPattern) &&
                            x.Title.Contains(searchPattern))
                .Select(x => x.TagId);

            var foundImages = DatabaseContext.ImageProperties
                .AsNoTracking()
                .Where(x => x.IsActive &&
                            tagIds.Contains(x.TagId))
                .Select(x => x.ImageId)
                .Join(
                DatabaseContext.Images,
                x => x,
                image => image.Id,
                (x, image) => new
                {
                    ImageId = image.Id,
                    IsActive = image.IsActive,
                })
                .Where(x => x.IsActive)
                .Select(x => x.ImageId)
                .Join(
                DatabaseContext.Reactions,
                x => x,
                reaction => reaction.ImageId,
                (x, reaction) => new
                {
                    IsActive = reaction.IsActive,
                    IsLiked = reaction.IsLiked,
                    ImageId = reaction.ImageId,
                })
                .Where(x => x.IsActive &&
                            x.IsLiked == isLiked)
                .Select(x => x.ImageId);

            var foundImagesCount = await foundImages.CountAsync();

            if (foundImagesCount == 0)
            {
                foundImages = DatabaseContext.Reactions
                    .AsNoTracking()
                    .Join(
                    DatabaseContext.Images,
                    reaction => reaction.ImageId,
                    image => image.Id,
                    (reaction, image) => new
                    {
                        ImageId = image.Id,
                        IsActiveReaction = reaction.IsActive,
                        IsActiveImage = image.IsActive,
                        IsLiked = reaction.IsLiked,
                        Title = image.Title,
                        UserId = reaction.AccountId,
                    })
                    .Where(x => x.IsActiveImage &&
                                x.IsActiveReaction &&
                                x.IsLiked == isLiked &&
                                x.UserId == userId &&
                                (string.IsNullOrEmpty(searchPattern) ||
                                x.Title.Contains(searchPattern)))
                    .Select(x => x.ImageId);
            }

            var images = await DatabaseContext.Images
                .AsNoTracking()
                .Where(x => foundImages.Contains(x.Id))
                .OrderByDescending(x => x.Id)
                .Skip(skip)
                .Select(x => new PreviewImageModel()
                {
                    ImageId = x.Id,
                    Title = x.Title,
                })
                .Take(ConstantsHelper.PageSize)
                .ToListAsync();

            foreach (var image in images)
            {
                try
                {
                    image.Image = _blobStorageHelper
                        .DownloadImage(image.ImageId.ToString());
                }
                catch { }
            }

            result.Images = images;
        }
        else if (tagId is not null &&
                 isLiked is not null)
        {
            var imageIds = DatabaseContext.Reactions
                .AsNoTracking()
                .Where(x => x.IsActive &&
                            x.AccountId == userId &&
                            x.IsLiked == isLiked)
                .Select(x => x.ImageId)
                .Join(
                DatabaseContext.Images.AsNoTracking(),
                x => x,
                image => image.Id,
                (x, image) => new
                {
                    IsActive = image.IsActive,
                    ImageId = image.Id,
                    Title = image.Title,
                })
                .Where(x => x.IsActive &&
                            (string.IsNullOrEmpty(searchPattern) ||
                            x.Title.Contains(searchPattern)))
                .Select(x => x.ImageId)
                .Join(
                DatabaseContext.ImageProperties.AsNoTracking(),
                x => x,
                property => property.ImageId,
                (x, property) => new
                {
                    ImageId = property.ImageId,
                    TagId = property.TagId,
                    IsActive = property.IsActive,
                })
                .Where(x => x.IsActive &&
                            x.TagId == tagId)
                .Join(
                DatabaseContext.Tags.AsNoTracking(),
                x => x.TagId,
                tag => tag.Id,
                (x, tag) => new
                {
                    ImageId = x.ImageId,
                    IsActive = tag.IsActive,
                })
                .Where(x => x.IsActive)
                .Select(x => x.ImageId);

            var images = await DatabaseContext.Images
                .AsNoTracking()
                .Where(x => imageIds.Contains(x.Id))
                .OrderByDescending(x => x.Id)
                .Skip(skip)
                .Select(x => new PreviewImageModel()
                {
                    ImageId = x.Id,
                    Title = x.Title,
                })
                .Take(ConstantsHelper.PageSize)
                .ToListAsync();

            foreach (var image in images)
            {
                try
                {
                    image.Image = _blobStorageHelper
                        .DownloadImage(image.ImageId.ToString());
                }
                catch { }
            }

            result.Images = images;
        }

        return result;
    }

    public async Task<ShowImageModel> CreateShowImageModel(
        int userId,
        int? imageId = null)
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

            var reaction = await DatabaseContext.Reactions
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.AccountId == userId &&
                                          x.ImageId == imageId);

            result.IsLiked = reaction?.IsLiked;
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

    public async Task<PreviewImagesModel> GetTagsPreviews(int userId, int? skip)
    {
        var result = new PreviewImagesModel();

        var tagQuery = DatabaseContext.Reactions
            .AsNoTracking()
            .Where(x => x.IsActive &&
                        x.AccountId == userId &&
                        x.IsLiked)
            .Select(x => x.ImageId)
            .Join(
            DatabaseContext.ImageProperties.AsNoTracking(),
            x => x,
            property => property.ImageId,
            (x, property) => new
            {
                IsActiveProperty = property.IsActive,
                TagId = property.TagId,
            })
            .Where(x => x.IsActiveProperty)
            .Join(
            DatabaseContext.Tags.AsNoTracking(),
            x => x.TagId,
            tag => tag.Id,
            (x, tag) => new
            {
                IsActiveTag = tag.IsActive,
                TagId = tag.Id,
            })
            .Where(x => x.IsActiveTag)
            .Select(x => x.TagId)
            .Distinct();

        result.Total = await tagQuery.CountAsync();

        var tagIds = await tagQuery
            .OrderByDescending(x => x)
            .Skip(skip ?? 0)
            .Take(ConstantsHelper.PageSize)
            .ToListAsync();

        var query = DatabaseContext.ImageProperties
            .AsNoTracking()
            .Where(x => x.IsActive &&
                        tagIds.Contains(x.TagId))
            .Join(
            DatabaseContext.Tags.AsNoTracking(),
            property => property.TagId,
            tag => tag.Id,
            (property, tag) => new
            {
                IsActiveTag = tag.IsActive,
                ImageId = property.ImageId,
                Title = tag.Title,
                TagId = tag.Id
            })
            .Where(x => x.IsActiveTag)
            .Join(
            DatabaseContext.Images.AsNoTracking(),
            x => x.ImageId,
            image => image.Id,
            (x, image) => new
            {
                IsActiveImage = image.IsActive,
                ImageId = image.Id,
                Title = x.Title,
                TagId = x.TagId,
            })
            .Where(x => x.IsActiveImage);

        var models = new List<PreviewTagModel>();

        foreach (var tag in tagIds)
        {
            var model = await query.FirstOrDefaultAsync(x => x.TagId == tag);

            models.Add(new PreviewTagModel()
            {
                ImageId = model.ImageId,
                Title = model.Title,
                TagId = model.TagId,
            });
        }

        foreach (var model in models)
        {
            try
            {
                model.Image = _blobStorageHelper
                    .DownloadImage(model.ImageId.ToString());
            }
            catch { }
        }

        result.Images = models;

        return result;
    }
}
