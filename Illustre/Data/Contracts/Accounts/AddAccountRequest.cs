using Data.Entities;

namespace Data.Contracts.Accounts;

public class AddAccountRequest : SignUpRequest
{
    public Role Role { get; set; }
}
