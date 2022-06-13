using Data.Contracts.Common;

namespace Data.Contracts.Media;

public class ManageImagesModel : IManageEntitiesModel
{
    public IEnumerable<ManageImageModel> Models { get; set; }

    public int Total { get; set; }

    public int Selected { get; set; }
}
