using Data.Contracts.Accounts;
using Data.Entities;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Data.Repositories;

public class AccountRepository
{
    private const int TokenDaysLife = 1;
    private const int SaltSize = 16;
    private readonly DatabaseContext _databaseContext;

    public AccountRepository(DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    public async Task<Role?> TryGetRoleBySessionGuid(string sessionGuid)
    {
        var startTime = DateTime.UtcNow.AddDays(-TokenDaysLife);
        return (await _databaseContext.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SessionGuid != null &&
                                      x.SessionGuid == sessionGuid &&
                                     (x.LastLogin == null ||
                                      x.LastLogin!.Value > startTime)))
            ?.Role;
    }

    public async Task<SignInResponse?> TrySignIn(SignInRequest request)
    {
        var email = request.Email.ToLower();
        var account = await _databaseContext.Accounts
        .FirstOrDefaultAsync(x => x.Email == email &&
                                  x.IsActive);

        if (account != null)
        {
            string passwordHash = GetPasswordHash(
                request.Password,
                Convert.FromHexString(account.Salt));

            if (passwordHash == account.PasswordHash)
            {
                return await GetSignInResponse(account);
            }
        }

        return null;
    }

    public async Task<SignInResponse?> TrySignUp(SignUpRequest request)
    {
        var account = CreateAccount(request.Email,
                                    request.Password,
                                    request.Username);

        try
        {
            await _databaseContext.Accounts.AddAsync(account);
            var result = await GetSignInResponse(account);
            await _databaseContext.SaveChangesAsync();

            return result;
        }
        catch
        {
            return null;
        }
    }

    public async Task<SignUpRequest?> TryGetAccount(string sessionGuid)
    {
        return await _databaseContext.Accounts
            .AsNoTracking()
            .Where(x => x.SessionGuid != null &&
                        x.SessionGuid == sessionGuid)
            .Select(x => new SignUpRequest()
            {
                Email = x.Email,
                Username = x.Username,
            })
            .FirstOrDefaultAsync();
    }

    public async Task<SignUpRequest?> TryUpdateAccount(string sessionGuid, UpdateAccountRequest request)
    {
        var account = await _databaseContext.Accounts
            .Where(x => x.SessionGuid != null &&
                        x.SessionGuid == sessionGuid)
            .FirstOrDefaultAsync();

        if (account == null)
        {
            return null;
        }

        if (request.Email is not null)
        {
            account.Email = request.Email;
        }

        if (request.Username is not null)
        {
            account.Username = request.Username;
        }

        if (request.Password is not null)
        {
            var passwordHash = GetPasswordHash(
                request.Password,
                Convert.FromHexString(account.Salt));

            account.PasswordHash = passwordHash;
        }

        try
        {
            await _databaseContext.SaveChangesAsync();
        }
        catch
        {
            return null;
        }

        return new SignUpRequest()
        {
            Email = account.Email,
            Username = account.Username,
        };
    }

    public async Task<ManageAccountsModel> GetEditors(int skip)
    {
        var result = new ManageAccountsModel();

        result.Total = await _databaseContext.Accounts
            .AsNoTracking()
            .CountAsync(x => x.Role == Role.Editor);
        result.Models = await _databaseContext.Accounts
            .AsNoTracking()
            .Where(x => x.Role == Role.Editor)
            .OrderBy(x => x.Id)
            .Skip(skip)
            .Take(10)
            .Select(x => new ManageAccountModel()
            {
                Id = x.Id,
                IsActive = x.IsActive,
                Email = x.Email,
                Username = x.Username,
                Role = x.Role,
            })
            .ToListAsync();

        return result;
    }

    public async Task<bool> TryAddAccount(AddAccountRequest request)
    {
        var account = CreateAccount(request.Email,
                                    request.Password,
                                    request.Username,
                                    request.Role);

        try
        {
            await _databaseContext.Accounts.AddAsync(account);
            await GetSignInResponse(account);
            await _databaseContext.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> TryUpdateAccountById(ManageAccountModel model)
    {
        var account = await _databaseContext.Accounts
            .FirstOrDefaultAsync(x => x.Id == model.Id);

        if (account == null)
        {
            return false;
        }

        account.IsActive = model.IsActive;

        if (model.Email is not null)
        {
            account.Email = model.Email;
        }

        if (model.Username is not null)
        {
            account.Username = model.Username;
        }

        if (model.Password is not null)
        {
            var passwordHash = GetPasswordHash(
                model.Password,
                Convert.FromHexString(account.Salt));

            account.PasswordHash = passwordHash;
        }

        try
        {
            await _databaseContext.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private Account CreateAccount(
        string email,
        string password,
        string username,
        Role role = Role.User)
    {
        var salt = GetSalt();
        var passwordHash = GetPasswordHash(password, salt);

        return new Account()
        {
            Email = email.ToLower(),
            Salt = Convert.ToHexString(salt),
            PasswordHash = passwordHash,
            Username = username,
            Role = role,
        };
    }

    private async Task<SignInResponse> GetSignInResponse(Account account)
    {
        var sessionGuid = SetSessionGuid(account);
        account.LastLogin = DateTime.UtcNow;

        await _databaseContext.SaveChangesAsync();
        return new SignInResponse()
        {
            Username = account.Username,
            Role = account.Role,
            SessionGuid = sessionGuid,
            Expires = account.LastLogin!.Value.AddDays(TokenDaysLife),
        };
    }

    private byte[] GetSalt()
    {
        return RandomNumberGenerator.GetBytes(SaltSize);
    }

    private string GetPasswordHash(string password, byte[] salt)
    {
        return Convert.ToHexString(
            KeyDerivation.Pbkdf2(
                password,
                salt,
                KeyDerivationPrf.HMACSHA256,
                100000,
                256 / 8));
    }

    private string GetGuid()
    {
        return Guid.NewGuid().ToString("N");
    }

    private string SetSessionGuid(Account account)
    {
        var sessionGuid = GetGuid();
        while (true)
        {
            try
            {
                account.SessionGuid = sessionGuid;
                break;
            }
            catch
            {
                sessionGuid = GetGuid();
            }
        }
        return sessionGuid;
    }
}
