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
}
