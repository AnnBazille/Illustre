using Data.Contracts.Common;

namespace Data.Contracts.Media;

public class ManageImagesRequest : FailableRequest, ISearchable
{
    public int Skip { get; set; } = 0;

    public ManageImagesModel? ImagesData { get; set; }

    public string? SearchPattern { get; set; }

    public string? Action { get; set; }

    public int? TagId { get; set; }

    public int? ImageId { get; set; }
}
