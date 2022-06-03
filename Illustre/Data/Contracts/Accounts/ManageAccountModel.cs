using Data.Contracts.Common;
using Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace Data.Contracts.Accounts;

public class ManageAccountModel : FailableRequest
{
    [Required]
    public int Id { get; set; }

    [Required]
    public bool IsActive { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public string? Username { get; set; }

    [Required]
    public Role Role { get; set; }
}
