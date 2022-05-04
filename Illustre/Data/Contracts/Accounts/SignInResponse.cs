using Data.Entities;

namespace Data.Contracts.Accounts;

public class SignInResponse
{
    public string Username { get; set; }

    public Role Role { get; set; }

    public string SessionGuid { get; set; }

    public DateTime Expires { get; set; }
}
