using Data.Entities.Common;

namespace Data.Entities;

public class Account : BaseEntity
{
    public string Email { get; set; }

    public string Salt { get; set; }

    public string PasswordHash { get; set; }

    public string Username { get; set; }

    public Role Role { get; set; }

    public string? SessionGuid { get; set; }

    public DateTime? LastLogin { get; set; }
}
