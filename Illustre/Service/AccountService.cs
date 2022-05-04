﻿using Data;
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
}