using Data.Contracts.Common;
using System.ComponentModel.DataAnnotations;

namespace Data.Contracts.Media;

public class ManageImageModel : FailableRequest
{
    [Required]
    public int Id { get; set; }

    [Required]
    public bool IsActive { get; set; }

    public string? Title { get; set; }
}
