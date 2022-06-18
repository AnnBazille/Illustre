using System.ComponentModel.DataAnnotations;

namespace Data.Contracts.Media;

public class SearchModel
{
    public int UserId { get; set; }

    [Required]
    public string SearchPattern { get; set; }

    public int? TagId { get; set; }

    public int Skip { get; set; } = 0;

    public bool? IsLiked { get; set; }
}
