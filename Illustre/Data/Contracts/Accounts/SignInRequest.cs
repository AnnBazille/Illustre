using Data.Contracts.Common;
using System.ComponentModel.DataAnnotations;

namespace Data.Contracts.Accounts;

public class SignInRequest : FailableRequest
{
    [Required]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }
}
