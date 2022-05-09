namespace Data.Contracts.Accounts;

public class ManageAccountsModel
{
    public IEnumerable<ManageAccountModel> Models { get; set; }

    public int Total { get; set; }
}
