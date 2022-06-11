using Azure.Storage.Blobs;
using Data.Contracts.Media;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;

namespace Data.Repositories;

public class MediaRepository : BaseRepository
{
    private const string ContainerName = "main-container";

    private const string TempDirectory = "temp";

    private readonly string BlobConnectionString;

    public MediaRepository(
        DatabaseContext databaseContext,
        IConfiguration configuration) : base(databaseContext)
    {
        BlobConnectionString = configuration.GetConnectionString("AzureBlobStorage");

        if (!Directory.Exists(TempDirectory))
        {
            Directory.CreateDirectory(TempDirectory);
        }
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

        Expression<Func<Image, bool>> predicate = x => string.IsNullOrEmpty(search) ||
                                                     x.Title.Contains(search);

        result.Total = await DatabaseContext.Images
            .AsNoTracking()
            .CountAsync(predicate);
        result.Models = await DatabaseContext.Images
            .AsNoTracking()
            .Where(predicate)
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
                BlobClient blob = new BlobClient(
                       BlobConnectionString,
                       ContainerName,
                       item.Id.ToString());

                var content = await blob.DownloadAsync();

                using var memoryStream = new MemoryStream();

                await content.Value.Content.CopyToAsync(memoryStream);

                var bytes = memoryStream.ToArray();

                var imageBase64Data = Convert.ToBase64String(bytes);

                item.Image = string.Format("data:image/png;base64,{0}", imageBase64Data);
                item.Image = item.Image.Replace("\r", "");
                item.Image = item.Image.Replace("\n", "");
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

            BlobContainerClient blobContainerClient = new BlobContainerClient(
                BlobConnectionString,
                ContainerName);

            await blobContainerClient.CreateIfNotExistsAsync();

            BlobClient blob = blobContainerClient.GetBlobClient(image.Id.ToString());

            var filename = TempDirectory + image.Id.ToString();

            var stream = new FileStream(filename, FileMode.OpenOrCreate);

            await request.File.CopyToAsync(stream);

            stream.Close();

            await blob.UploadAsync(filename);

            File.Delete(filename);
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
                        x.ImageId == imageId)
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

    public async Task<bool> TryEditTagById(int tagId, int imageId)
    {
        var imageProperty = new ImageProperty()
        {
            TagId = tagId,
            ImageId = imageId,
        };

        try
        {
            await DatabaseContext.ImageProperties.AddAsync(imageProperty);
            await DatabaseContext.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }
}
