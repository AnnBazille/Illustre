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
        var expirationTime = DateTime.UtcNow.AddDays(TokenDaysLife);
        return (await _databaseContext.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SessionGuid != null &&
                                      x.SessionGuid == sessionGuid &&
                                     (x.LastLogin == null ||
                                      x.LastLogin!.Value < expirationTime)))
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
        var salt = GetSalt();
        var passwordHash = GetPasswordHash(request.Password, salt);

        var account = new Account()
        {
            Email = request.Email.ToLower(),
            Salt = Convert.ToHexString(salt),
            PasswordHash = passwordHash,
            Username = request.Username,
            Role = Role.User,
        };

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
