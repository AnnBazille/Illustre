using Data.Contracts.Common;

namespace Data.Contracts.Accounts;

public class ManageAccountsRequest : FailableRequest, ISearchable
{
    public int Skip { get; set; } = 0;

    public ManageAccountsModel? AccountsData { get; set; }

    public string? SearchPattern { get; set; }

    public string? Action { get; set; }

    public int? TagId { get; set; }

    public int? ImageId { get; set; }
}
