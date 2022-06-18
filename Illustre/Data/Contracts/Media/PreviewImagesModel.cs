namespace Data.Contracts.Media;

public class PreviewImagesModel
{
    public IEnumerable<PreviewImageModel> Images { get; set; }

    public int Total { get; set; }

    public SearchModel? SearchModel { get; set; }
}
