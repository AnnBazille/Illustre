using Data.Contracts.Media;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Data.Repositories;

public class MediaRepository : BaseRepository
{
    public MediaRepository(DatabaseContext databaseContext) : base(databaseContext) { }

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
}
