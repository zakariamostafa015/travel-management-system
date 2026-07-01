using System.Security.Cryptography;
using System.Text;
using TravelToursWebsite.Application.Features.Auth;

namespace TravelToursWebsite.Infrastructure.Auth;

public sealed class LegacyPasswordHasher : IPasswordHasher
{
    private const int SaltSize = 32;
    private const int HashSize = 32;
    private const int Iterations = 10000;

    public string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[SaltSize];
        rng.GetBytes(salt);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(HashSize);

        var saltAndHash = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, saltAndHash, 0, SaltSize);
        Array.Copy(hash, 0, saltAndHash, SaltSize, HashSize);

        return Convert.ToBase64String(saltAndHash);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        try
        {
            var saltAndHash = Convert.FromBase64String(passwordHash);
            if (saltAndHash.Length != SaltSize + HashSize)
            {
                return false;
            }

            var salt = new byte[SaltSize];
            var hash = new byte[HashSize];
            Array.Copy(saltAndHash, 0, salt, 0, SaltSize);
            Array.Copy(saltAndHash, SaltSize, hash, 0, HashSize);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            var testHash = pbkdf2.GetBytes(HashSize);

            return CryptographicOperations.FixedTimeEquals(hash, testHash);
        }
        catch (FormatException)
        {
            return false;
        }
        catch (CryptographicException)
        {
            return false;
        }
    }
}