using Data.Contracts.Common;

namespace Data.Contracts.Media;

public class ManageTagsModel : IManageEntitiesModel
{
    public IEnumerable<ManageTagModel> Models { get; set; }

    public int Total { get; set; }

    public int Selected { get; set; }
}
