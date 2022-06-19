using Data.Contracts.Media;
using Data.Repositories;

namespace Service;

public class MediaService
{
    private readonly MediaRepository _mediaRepository;

    public MediaService(MediaRepository mediaRepository)
    {
        _mediaRepository = mediaRepository;
    }

    public async Task<ManageTagsModel> GetTags(int skip, string? search)
    {
        return await _mediaRepository.GetTags(skip, search);
    }

    public async Task<bool> TryAddTag(AddTagRequest request)
    {
        return await _mediaRepository.TryAddTag(request);
    }

    public async Task<bool> TryUpdateTagById(ManageTagModel model)
    {
        return await _mediaRepository.TryUpdateTagById(model);
    }

    public async Task<ManageImagesModel> GetImages(int skip, string? search)
    {
        return await _mediaRepository.GetImages(skip, search);
    }

    public async Task<bool> TryAddImage(AddImageRequest request)
    {
        return await _mediaRepository.TryAddImage(request);
    }

    public async Task<bool> TryUpdateImageById(ManageImageModel model)
    {
        return await _mediaRepository.TryUpdateImageById(model);
    }

    public async Task<ManageTagsModel> GetEditableTags(int skip, string? search, int imageId)
    {
        return await _mediaRepository.GetEditableTags(skip, search, imageId);
    }

    public async Task<bool> TryEditTagById(EditTagModel model)
    {
        return await _mediaRepository.TryAddImageProperty(model.Id, model.ImageId, model.IsActive);
    }

    public async Task<ManageImagesModel> GetEditableImages(int skip, string? search, int tagId)
    {
        return await _mediaRepository.GetEditableImages(skip, search, tagId);
    }

    public async Task<bool> TryEditImageById(EditImageModel model)
    {
        return await _mediaRepository.TryAddImageProperty(model.TagId, model.Id, model.IsActive);
    }

    public async Task<ShowImageModel> GetNextImage(int userId)
    {
        return await _mediaRepository.GetNextImage(userId);
    }

    public async Task SetReaction(SetReactionModel model)
    {
        await _mediaRepository
            .SetReaction(model.UserId, model.ImageId, model.IsLiked);
    }

    public async Task<PreviewImagesModel> GetImagePreviews(
        SearchModel model)
    {
        return await _mediaRepository.GetImagePreviews(
            model.UserId,
            model.SearchPattern,
            model.TagId,
            model.Skip,
            model.IsLiked);
    }

    public async Task<ShowImageModel> GetImage(int userId, int imageId)
    {
        return await _mediaRepository.CreateShowImageModel(userId, imageId);
    }
}
