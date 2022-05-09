namespace Data.Contracts.Accounts;

public class ManageEditorsRequest : FailableRequest
{
    public int Skip { get; set; } = 0;

    public ManageAccountsModel? AccountsData { get; set; }
}
