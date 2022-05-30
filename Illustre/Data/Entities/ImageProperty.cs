using Data.Entities.Common;

namespace Data.Entities;

public class ImageProperty : BaseEntity
{
    public int ImageId { get; set; }

    public Image Image { get; set; }

    public int TagId { get; set; }

    public Tag Tag { get; set; }
}
