namespace Data.Contracts.Media;

public class ShowImageModel : PreviewImageModel
{
    public bool? IsLiked { get; set; }

    public IEnumerable<ShowTagModel> Tags { get; set; }
}
