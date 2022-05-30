using Data.Contracts.Common;
using System.ComponentModel.DataAnnotations;

namespace Data.Contracts.Media;

public class AddTagRequest : FailableRequest
{
    [Required]
    public string Title { get; set; }
}
