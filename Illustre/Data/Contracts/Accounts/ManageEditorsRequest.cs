namespace Data.Contracts.Accounts;

public class ManageEditorsRequest : FailableRequest, ISearchable
{
    public int Skip { get; set; } = 0;

    public ManageAccountsModel? AccountsData { get; set; }

    public string? SearchPattern { get; set; }
}
