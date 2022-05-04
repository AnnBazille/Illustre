namespace Data.Entities;

public class Reaction : BaseEntity
{
    public int AccountId { get; set; }

    public Account Account { get; set; }

    public int ImageId { get; set; }

    public Image Image { get; set; }

    public bool IsLiked { get; set; }
}
