namespace Data.Contracts.Media;

public class ManageTagsModel
{
    public IEnumerable<ManageTagModel> Models { get; set; }

    public int Total { get; set; }
}
