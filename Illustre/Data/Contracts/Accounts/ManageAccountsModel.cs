using Data.Contracts.Common;

namespace Data.Contracts.Accounts;

public class ManageAccountsModel : IManageEntitiesModel
{
    public IEnumerable<ManageAccountModel> Models { get; set; }

    public int Total { get; set; }

    public int Selected { get; set; }
}
