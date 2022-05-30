using Data.Contracts.Common;

namespace Data.Contracts.Accounts;

public class UpdateAccountRequest : FailableRequest
{
    public string? Email { get; set; }

    public string? Password { get; set; }

    public string? Username { get; set; }
}
