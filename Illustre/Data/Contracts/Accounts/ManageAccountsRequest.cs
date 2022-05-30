using Data.Contracts.Common;

namespace Data.Contracts.Accounts;

public class ManageAccountsRequest : FailableRequest, ISearchable
{
    public int Skip { get; set; } = 0;

    public ManageAccountsModel? AccountsData { get; set; }

    public string? SearchPattern { get; set; }
}
