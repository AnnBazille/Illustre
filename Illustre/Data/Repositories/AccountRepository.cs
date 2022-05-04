using Data.Contracts.Accounts;
using Data.Entities;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories;

public class AccountRepository
{
    private const int TokenDaysLife = 1;
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
        var account = await _databaseContext.Accounts
        .FirstOrDefaultAsync(x => x.Email == request.Email &&
                                  x.IsActive);

        if (account != null)
        {
            string passwordHash = Convert.ToHexString(KeyDerivation.Pbkdf2(
                password: request.Password,
                salt: Convert.FromHexString(account.Salt),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            if (passwordHash == account.PasswordHash)
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
        }

        return null;
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
