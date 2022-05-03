using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

const int length = 14;
var uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToArray();
var lowercase = "abcdefghijklmnopqrstuvwxyz".ToArray();
var digits = "0123456789".ToArray();
var special = "~!@#$%^&*_-+=`|\\(){}[]:;\"'<>,.?/".ToArray();

while (true)
{
    Console.WriteLine("Any key - generate new password");
    Console.WriteLine("Esc - exit");
    Console.WriteLine();
    var key = Console.ReadKey(true);

    if (key.Key != ConsoleKey.Escape)
    {
        var password = new StringBuilder();
        var random = new Random();

        var uppercaseIndex = random.Next(length);
        var lowercaseIndex = random.Next(length);
        while (lowercaseIndex == uppercaseIndex)
        {
            lowercaseIndex = random.Next(length);
        }
        var digitsIndex = random.Next(length);
        while (digitsIndex == uppercaseIndex ||
               digitsIndex == lowercaseIndex)
        {
            digitsIndex = random.Next(length);
        }
        var specialIndex = random.Next(length);
        while (specialIndex == uppercaseIndex ||
               specialIndex == lowercaseIndex ||
               specialIndex == digitsIndex)
        {
            specialIndex = random.Next(length);
        }

        for (int i = 0; i < length; i++)
        {
            if (i == uppercaseIndex)
            {
                var index = random.Next(uppercase.Length);
                password.Append(uppercase[index]);
            }
            else if (i == lowercaseIndex)
            {
                var index = random.Next(lowercase.Length);
                password.Append(lowercase[index]);
            }
            else if (i == digitsIndex)
            {
                var index = random.Next(digits.Length);
                password.Append(digits[index]);
            }
            else if (i == specialIndex)
            {
                var index = random.Next(special.Length);
                password.Append(special[index]);
            }
            else
            {
                var type = random.Next(4);
                var data = type switch
                {
                    0 => uppercase,
                    1 => lowercase,
                    2 => digits,
                    3 => special,
                };
                var index = random.Next(data.Length);
                password.Append(data[index]);
            }
        }

        var salt = RandomNumberGenerator.GetBytes(16);
        string passwordHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password.ToString(),
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));

        Console.WriteLine($"password: {password.ToString()}");
        Console.WriteLine($"salt: {Convert.ToBase64String(salt)}");
        Console.WriteLine($"password hash: {passwordHash}");
        Console.WriteLine();
    }
    else
    {
        break;
    }
}
