using Data.Contracts.Common;

namespace Data.Contracts.Media;

public class ManageTagModel : FailableRequest
{
    public int Id { get; set; }

    public bool IsActive { get; set; }

    public string Title { get; set; }
}
