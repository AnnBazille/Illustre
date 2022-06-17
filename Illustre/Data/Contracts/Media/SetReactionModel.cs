namespace Data.Contracts.Media;

public class SetReactionModel
{
    public int UserId { get; set; }

    public int ImageId { get; set; }

    public bool IsLiked { get; set; }
}
