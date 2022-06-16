namespace Data.Contracts.Media;

public class ShowImageModel
{
    public int ImageId { get; set; }

    public string Image { get; set; }

    public string Title { get; set; }

    public bool? IsLiked { get; set; }

    public IEnumerable<ShowTagModel> Tags { get; set; }
}
