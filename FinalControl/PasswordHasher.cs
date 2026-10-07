using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace FinalControl.Data;

internal readonly record struct PasswordVerificationResult(bool IsValid, bool NeedsUpgrade);

internal static class PasswordHasher
{
    private const string Algorithm = "PBKDF2-SHA256";
    private const int Iterations = 600_000;
    private const int MinimumAcceptedIterations = 100_000;
    private const int MaximumAcceptedIterations = 1_000_000;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    internal static string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var derivedKey = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        return string.Join(
            '$',
            Algorithm,
            Iterations.ToString(CultureInfo.InvariantCulture),
            Convert.ToBase64String(salt),
            Convert.ToBase64String(derivedKey));
    }

    internal static PasswordVerificationResult Verify(string storedHash, string password)
    {
        if (string.IsNullOrEmpty(storedHash) || string.IsNullOrEmpty(password))
        {
            return default;
        }

        if (storedHash.StartsWith(Algorithm + "$", StringComparison.Ordinal))
        {
            return VerifyPbkdf2(storedHash, password);
        }

        return VerifyLegacySha256(storedHash, password);
    }

    private static PasswordVerificationResult VerifyPbkdf2(string storedHash, string password)
    {
        var parts = storedHash.Split('$');
        if (parts.Length != 4
            || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var iterations)
            || iterations < MinimumAcceptedIterations
            || iterations > MaximumAcceptedIterations)
        {
            return default;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expectedKey = Convert.FromBase64String(parts[3]);
            if (salt.Length < SaltSize || salt.Length > 64 || expectedKey.Length != HashSize)
            {
                return default;
            }

            var actualKey = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                HashSize);

            var isValid = CryptographicOperations.FixedTimeEquals(actualKey, expectedKey);
            return new PasswordVerificationResult(isValid, isValid && iterations < Iterations);
        }
        catch (FormatException)
        {
            return default;
        }
    }

    private static PasswordVerificationResult VerifyLegacySha256(string storedHash, string password)
    {
        try
        {
            var expectedHash = Convert.FromHexString(storedHash);
            var actualHash = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            var isValid = expectedHash.Length == actualHash.Length
                && CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);

            return new PasswordVerificationResult(isValid, isValid);
        }
        catch (FormatException)
        {
            return default;
        }
    }
}
