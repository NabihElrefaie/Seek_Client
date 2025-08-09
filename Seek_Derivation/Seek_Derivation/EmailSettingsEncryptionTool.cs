using Seek_Derivation;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

public static class EmailSettingsEncryptionTool
{
    public static void Run(string[] args)
    {
        Console.Clear();
        Console.WriteLine("╔══════════════════════════════════════╗");
        Console.WriteLine("║         ENCRYPTION TOOL v1.0         ║");
        Console.WriteLine("╚══════════════════════════════════════╝");

        try
        {
            string targetDir = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
            var settings = GetEmailSettings();

            // 1️⃣ Auto-generate a strong password
            string masterPassword = GenerateStrongPassword(32);
            byte[] key = CryptoHelper.GenerateKey(masterPassword);

            // 2️⃣ Encrypt settings
            string json = JsonSerializer.Serialize(settings);
            byte[] encryptedConfig = CryptoHelper.Encrypt(key, json);
            byte[] encryptedKey = CryptoHelper.Encrypt(key, Convert.ToBase64String(key));

            // 3️⃣ Save files
            Directory.CreateDirectory(targetDir);
            string configPath = Path.Combine(targetDir, "email.cfg");
            string keyPath = Path.Combine(targetDir, "email.key");

            File.WriteAllBytes(configPath, encryptedConfig);
            File.WriteAllBytes(keyPath, encryptedKey);

            Console.WriteLine($"\n[✓] Files saved to: {targetDir}");
            Console.WriteLine($" • email.cfg");
            Console.WriteLine($" • email.key");

            // 4️⃣ Send the master password to admin
            Console.WriteLine("\n[✓] Sending master password to admin...");
            EmailService.SendPasswordToAdmin(settings, masterPassword);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[ERROR] {ex.Message}");
        }
    }

    private static EmailSettings GetEmailSettings()
    {
        var settings = new EmailSettings();

        Console.WriteLine("\nSMTP Configuration:");
        Console.Write("Server (e.g., smtp.gmail.com): ");
        settings.SmtpServer = Console.ReadLine();

        Console.Write("Port (587/465): ");
        if (int.TryParse(Console.ReadLine(), out int port)) settings.SmtpPort = port;

        Console.Write("Use SSL? (Y/n): ");
        settings.UseSsl = !Console.ReadLine().Equals("n", StringComparison.OrdinalIgnoreCase);

        Console.WriteLine("\nAccount Details:");
        Console.Write("Username: ");
        settings.FromEmail = Console.ReadLine();
        settings.Username = settings.FromEmail;

        Console.Write("Password: ");
        settings.Password = GetMaskedInput();

        Console.Write("Display Name: ");
        settings.DisplayName = Console.ReadLine();

        Console.Write("\nAdmin Email: ");
        settings.AdminEmail = Console.ReadLine();

        return settings;
    }

    private static string GetMaskedInput()
    {
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

    private static string GenerateStrongPassword(int length = 32)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()_+-=[]{}";
        using var rng = RandomNumberGenerator.Create();
        var data = new byte[length];
        rng.GetBytes(data);
        return new string(data.Select(b => chars[b % chars.Length]).ToArray());
    }
}
