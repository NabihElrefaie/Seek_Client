// EmailSettingsDecryptionTool.cs
using Seek_Derivation;
using System.Text;
using System.Text.Json;

public static class EmailSettingsDecryptionTool
{
    public static void Run(string[] args)
    {
        Console.Clear();
        Console.WriteLine("╔══════════════════════════════════════╗");
        Console.WriteLine("║         DECRYPTION TOOL v1.0         ║");
        Console.WriteLine("╚══════════════════════════════════════╝");

        try
        {
            string targetDir = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
            string configPath = Path.Combine(targetDir, "email.cfg");
            string keyPath = Path.Combine(targetDir, "email.key");

            if (!File.Exists(configPath) || !File.Exists(keyPath))
                throw new FileNotFoundException("Encrypted files not found in: " + targetDir);

            Console.Write("\nEnter decryption password: ");
            string password = GetMaskedInput();

            byte[] key = CryptoHelper.GenerateKey(password);
            byte[] encryptedKey = File.ReadAllBytes(keyPath);
            string decryptedKey = CryptoHelper.Decrypt(key, encryptedKey);

            byte[] actualKey = Convert.FromBase64String(decryptedKey);
            string json = CryptoHelper.Decrypt(actualKey, File.ReadAllBytes(configPath));
            var settings = JsonSerializer.Deserialize<EmailSettings>(json);

            Console.WriteLine("\nDecrypted Settings:");
            Console.WriteLine($"SMTP: {settings.SmtpServer}:{settings.SmtpPort}");
            Console.WriteLine($"SSL: {settings.UseSsl}");
            Console.WriteLine($"From: {settings.FromEmail}");
            Console.WriteLine($"Admin: {settings.AdminEmail}");
            Console.WriteLine($"Password: {settings.Password}");

            Console.Write("\nSend test email? (Y/n): ");
            if (!Console.ReadLine().Equals("n", StringComparison.OrdinalIgnoreCase))
            {
                settings.Password = GetMaskedInput("Enter SMTP password: ");
                EmailService.SendTestEmail(settings);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError: {ex.Message}");
        }
    }

    private static string GetMaskedInput(string prompt = "")
    {
        if (!string.IsNullOrEmpty(prompt)) Console.Write(prompt);

        var sb = new StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter) break;
            if (key.Key == ConsoleKey.Backspace && sb.Length > 0)
            {
                sb.Remove(sb.Length - 1, 1);
                Console.Write("\b \b");
            }
            else if (!char.IsControl(key.KeyChar))
            {
                sb.Append(key.KeyChar);
                Console.Write("*");
            }
        }
        Console.WriteLine();
        return sb.ToString();
    }
}