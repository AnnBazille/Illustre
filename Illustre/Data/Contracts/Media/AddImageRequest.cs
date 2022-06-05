using Data.Contracts.Common;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Data.Contracts.Media;

public class AddImageRequest : FailableRequest
{
    [Required]
    public string Title { get; set; }

    [Required]
    public IFormFile File { get; set; }
}
