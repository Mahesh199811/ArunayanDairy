using System.Security.Cryptography;

namespace ArunayanDairy.API.Security;

/// <summary>
/// Password hashing using PBKDF2 with salt
/// </summary>
public class PasswordHasher
{
    private const int SaltSize = 16; // 128 bits
    private const int KeySize = 32; // 256 bits
    private const int Iterations = 100000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    private const char SegmentDelimiter = ':';

    /// <summary>
    /// Hash a password with a random salt
    /// </summary>
    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            Algorithm,
            KeySize
        );

        return string.Join(
            SegmentDelimiter,
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash)
        );
    }

    /// <summary>
    /// Verify a password against a hash
    /// </summary>
    public static bool Verify(string password, string passwordHash)
    {
        var segments = passwordHash.Split(SegmentDelimiter);
        if (segments.Length != 2)
            return false;

        var salt = Convert.FromBase64String(segments[0]);
        var hash = Convert.FromBase64String(segments[1]);

        var inputHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            Algorithm,
            KeySize
        );

        return CryptographicOperations.FixedTimeEquals(inputHash, hash);
    }
}
