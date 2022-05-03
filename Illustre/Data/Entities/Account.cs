namespace Data.Entities;

public class Account : BaseEntity
{
    public string Email { get; set; }

    public byte[] Salt { get; set; }

    public byte[] PasswordHash { get; set; }

    public string Username { get; set; }

    public Role Role { get; set; }

    public byte[]? SessionGuid { get; set; }

    public DateTime LastLogin { get; set; }
}
