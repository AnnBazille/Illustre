using Data.Entities;

namespace Data.Contracts.Accounts;

public class ManageAccountModel : FailableRequest
{
    public int Id { get; set; }

    public bool IsActive { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public string? Username { get; set; }

    public Role Role { get; set; }
}
