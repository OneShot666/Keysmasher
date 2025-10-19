using System.Text.Json.Serialization.Metadata;
using System.Security.Cryptography;
using System.Text.Json;
using Items;

namespace Security;
public static class CryptoUtils {                                               // Use for local saves and password hashing
    private static readonly byte[] secret_key = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("qiL1z'=8lwj5K°°.72R"));
    private static readonly byte[] InitVector = new byte[16];                   // Good for local game

    public static string GenerateSalt(int size=16) {                            // Return random salt
        byte[] saltBytes = RandomNumberGenerator.GetBytes(size);
        return Convert.ToBase64String(saltBytes);
    }

    public static string HashPassword(string password, string salt) {           // Return hashed password
        using var sha256 = SHA256.Create();
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(password + salt);
        byte[] hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    public static bool VerifyPassword(string password, string encryptSalt, string hash) {   // Check password is correct
        return HashPassword(password, encryptSalt) == EncryptText(hash);
    }

    public static string EncryptSave<T>(T data) {
        var options = new JsonSerializerOptions { WriteIndented = true,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver { Modifiers = {
            ti => { if (ti.Type == typeof(Item)) {
            ti.PolymorphismOptions ??= new JsonPolymorphismOptions {
            TypeDiscriminatorPropertyName = "$type" }; }}}}};
        string json = JsonSerializer.Serialize(data, options);
        return EncryptText(json);
    }

    public static T? DecryptSave<T>(string encryptedData) {                     // Remove prints after testing
        string json = DecryptText(encryptedData);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<T>(json, options);
    }

    private static string EncryptText(string plainText) {                       // plain : not crypted
        if (string.IsNullOrEmpty(plainText)) return plainText;

        using var aes = Aes.Create();
        aes.Key = secret_key;
        aes.IV = InitVector;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs)) { sw.Write(plainText); }

        return Convert.ToBase64String(ms.ToArray());
    }

    private static string DecryptText(string cipherText) {                      // cipher : crypted
        if (string.IsNullOrEmpty(cipherText)) return cipherText;

        using var aes = Aes.Create();
        aes.Key = secret_key;
        aes.IV = InitVector;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);

        return sr.ReadToEnd();
    }
}
