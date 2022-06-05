namespace Data.Contracts.Media;

public class ManageImagesModel
{
    public IEnumerable<ManageImageModel> Models { get; set; }

    public int Total { get; set; }
}
