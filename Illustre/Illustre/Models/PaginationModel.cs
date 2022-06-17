using Data.Contracts.Common;

namespace Illustre.Models;

public class PaginationModel : ISearchable
{
    public int? Total { get; set; }

    public int Skip { get; set; }

    public int? ImageId { get; set; }

    public int? TagId { get; set; }

    public string? SearchPattern { get; set; }

    public string Controller { get; set; }

    public string? Action { get; set; }
}
