using Data.Contracts.Accounts;
using Data.Entities;
using Data.Repositories;

namespace Service;

public class AccountService
{
    private readonly AccountRepository _accountRepository;

    public AccountService(AccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<Role?> TryGetRoleBySessionGuid(string sessionGuid)
    {
        return await _accountRepository.TryGetRoleBySessionGuid(sessionGuid);
    }

    public async Task<SignInResponse?> TrySignIn(SignInRequest request)
    {
        return await _accountRepository.TrySignIn(request);
    }

    public async Task<SignInResponse?> TrySignUp(SignUpRequest request)
    {
        return await _accountRepository.TrySignUp(request);
    }

    public async Task<SignUpRequest?> TryGetAccount(string sessionGuid)
    {
        return await _accountRepository.TryGetAccount(sessionGuid);
    }

    public async Task<SignUpRequest?> TryUpdateAccount(
        string sessionGuid,
        UpdateAccountRequest request)
    {
        return await _accountRepository.TryUpdateAccount(sessionGuid, request);
    }

    public async Task<ManageAccountsModel> GetEditors(int skip, string? search)
    {
        return await _accountRepository.GetAccounts(skip, search, Role.Editor);
    }

    public async Task<ManageAccountsModel> GetUsers(int skip, string? search)
    {
        return await _accountRepository.GetAccounts(skip, search, Role.User);
    }

    public async Task<bool> TryAddAccount(AddAccountRequest request)
    {
        return await _accountRepository.TryAddAccount(request);
    }

    public async Task<bool> TryUpdateAccountById(ManageAccountModel model)
    {
        return await _accountRepository.TryUpdateAccountById(model);
    }

    public async Task<int?> TryGetAccountIdBySessionGuid(string sessionGuid)
    {
        return await _accountRepository
            .TryGetAccountIdBySessionGuid(sessionGuid);
    }
}
