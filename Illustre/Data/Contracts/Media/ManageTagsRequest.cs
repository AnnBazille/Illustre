using Data.Contracts.Common;

namespace Data.Contracts.Media;

public class ManageTagsRequest : FailableRequest, ISearchable
{
    public int Skip { get; set; } = 0;

    public ManageTagsModel? TagsData { get; set; }

    public string? SearchPattern { get; set; }
}
