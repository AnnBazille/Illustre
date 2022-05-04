using System.ComponentModel.DataAnnotations;

namespace Data.Contracts.Accounts;

public class SignInRequest
{
    [Required]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }

    public bool IsFirstAttempt { get; set; } = true;
}
