using System.Security.Cryptography;
using System.Text;

public static class CryptoHelper
{
    private const int KeySize = 32;
    private static readonly byte[] Salt =
    {
        0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64,
        0x76, 0x65, 0x64, 0x65, 0x76, 0x54, 0x6f, 0x77
    };

    public static byte[] GenerateKey(string password, int iterations = 100000)
    {
        using var deriveBytes = new Rfc2898DeriveBytes(
            Encoding.UTF8.GetBytes(password),
            Salt,
            iterations,
            HashAlgorithmName.SHA512);
        return deriveBytes.GetBytes(KeySize);
    }

    public static byte[] Encrypt(byte[] key, string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();

        using var ms = new MemoryStream();
        ms.Write(aes.IV, 0, aes.IV.Length);

        using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs))
        {
            sw.Write(plainText);
        }
        return ms.ToArray();
    }

    public static string Decrypt(byte[] key, byte[] cipherText)
    {
        using var aes = Aes.Create();
        aes.Key = key;

        byte[] iv = new byte[16];
        Array.Copy(cipherText, 0, iv, 0, iv.Length);
        aes.IV = iv;

        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
        {
            cs.Write(cipherText, iv.Length, cipherText.Length - iv.Length);
        }
        return Encoding.UTF8.GetString(ms.ToArray());
    }
}