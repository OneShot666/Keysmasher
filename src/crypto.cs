using System.Security.Cryptography;

namespace Gameplay;
public static class CryptoUtils {
    private static readonly byte[] Key = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("0v#§8Up7ren3~74Xd,!"));
    private static readonly byte[] InitVector = new byte[16];                   // Good for local game

    public static string GenerateSalt(int size=16) {
        byte[] saltBytes = RandomNumberGenerator.GetBytes(size);
        return Convert.ToBase64String(saltBytes);
    }

    public static string HashPassword(string password, string? salt) {
        using (var sha256 = SHA256.Create()) {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(password + salt);
            byte[] hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }

    public static bool VerifyPassword(string password, string? Salt, string? hash) {
        return HashPassword(password, Salt) == hash;
    }
}
