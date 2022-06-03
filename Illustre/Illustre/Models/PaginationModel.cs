namespace Illustre.Models;

public class PaginationModel
{
    public int? Total { get; set; }

    public int Skip { get; set; }

    public string AspController { get; set; }

    public string AspAction { get; set; }
}
