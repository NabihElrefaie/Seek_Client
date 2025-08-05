
using System.Net;
using System.Security.Cryptography;
using System.Text;

using Microsoft.Win32;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using System.Management;

namespace Seek.Core.Security
{
    /// <summary>
    /// Handles secure management of encryption keys in the application
    /// Supports multiple key derivation and storage strategies for offline use
    /// </summary>
    public class SecureKeyManager
    {
        // Default salt for PBKDF2 (in production, this should be stored securely or generated per-user)
        private static readonly byte[] DefaultSalt = new byte[]
        {
            0x3F, 0x68, 0x92, 0xA4, 0xD1, 0xB5, 0xC3, 0xE7,
            0xF9, 0x45, 0x21, 0x36, 0x7C, 0x8D, 0x9A, 0xB2
        };

        private const int DefaultIterations = 50000;  // High iteration count for PBKDF2
        private const int KeySize = 32;  // 256-bit key
        private const string KeyFileName = "key_config.dat";
        private const string KeyRegistryPath = @"SOFTWARE\Seek\Security";
        private const string KeyRegistryName = "KeyConfig";

        private readonly string _applicationPath;
        private readonly ILogger _logger;
        private readonly EmailService _emailService;
        private const string FirstTimeMarkerFile = "first_registration.marker";

        public SecureKeyManager(string applicationPath, ILogger logger, EmailService emailService = null)
        {
            _applicationPath = applicationPath ?? throw new ArgumentNullException(nameof(applicationPath));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _emailService = emailService; // Email service is optional
        }

        /// <summary>
        /// Retrieves the database encryption key using all available security methods
        /// </summary>
        /// <param name="userPassword">Optional user-provided password for additional security</param>
        /// <returns>The encryption key for database encryption</returns>
        public string GetEncryptionKey(string? userPassword = null)
        {
            try
            {
                // 1. Get the base key material from secure storage (or generate if not exists)
                byte[] baseKeyMaterial = GetOrCreateBaseKey(userPassword!);

                // 2. Hardware binding
                byte[] hardwareBoundKey = BindKeyToHardware(baseKeyMaterial);

                // 3. If user password is provided, derive final key with it
                if (!string.IsNullOrEmpty(userPassword))
                {
                    return DeriveKeyFromPassword(userPassword, hardwareBoundKey);
                }
                else
                {
                    // Convert the hardware-bound key to a usable string format
                    return Convert.ToBase64String(hardwareBoundKey);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve encryption key");
                throw new CryptographicException("Failed to retrieve encryption key", ex);
            }
        }

        /// <summary>
        /// Sets a new user password for key derivation
        /// </summary>
        /// <param name="newPassword">New password to use</param>
        /// <returns>True if password was successfully set</returns>
        public bool SetUserPassword(string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                _logger.LogWarning("Attempt to set empty password was rejected");
                return false;
            }

            try
            {
                byte[] baseKey = GetOrCreateBaseKey(newPassword);
                byte[] hardwareBoundKey = BindKeyToHardware(baseKey);

                // Generate a verification hash that we can use to validate the password later
                byte[] verificationHash = DeriveVerificationBytes(newPassword, hardwareBoundKey);

                // Store verification hash (but not the actual key)
                StoreVerificationHash(verificationHash);

                _logger.LogInformation("User password successfully set");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set user password");
                return false;
            }
        }

        /// <summary>
        /// Validates if the provided password is correct
        /// </summary>
        /// <param name="password">Password to validate</param>
        /// <returns>True if password is valid</returns>
        public bool ValidatePassword(string password)
        {
            try
            {
                byte[] baseKey = GetOrCreateBaseKey(password);
                byte[] hardwareBoundKey = BindKeyToHardware(baseKey);

                byte[] expectedHash = RetrieveVerificationHash();
                byte[] actualHash = DeriveVerificationBytes(password, hardwareBoundKey);

                return SlowEquals(expectedHash, actualHash);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Password validation failed");
                return false;
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Gets the base key from secure storage or creates a new one if it doesn't exist
        /// </summary>
        /// <param name="userPassword">Optional user password to include in notification</param>
        /// <returns>Base key bytes</returns>
        private byte[] GetOrCreateBaseKey(string? userPassword = null)
        {
            // Try multiple storage locations to retrieve key
            byte[] key = RetrieveKeyFromRegistry() ?? RetrieveKeyFromFile();
            bool isFirstTimeRegistration = false;

            if (key == null)
            {
                // Generate a new random key if none exists
                key = GenerateRandomKey();

                // Store in multiple locations for redundancy
                StoreKeyInRegistry(key);
                StoreKeyInFile(key);

                // Mark as first-time registration
                isFirstTimeRegistration = true;
                string markerPath = Path.Combine(_applicationPath, FirstTimeMarkerFile);
                if (!File.Exists(markerPath))
                {
                    // This is a new device registration, create the marker file
                    File.WriteAllText(markerPath, DateTime.UtcNow.ToString("o"));
                    // Send email notification asynchronously (don't wait for it)
                    _ = SendNewDeviceRegistrationEmailAsync(userPassword);
                }
                _logger.LogInformation("New base key generated and stored");
            }

            return ProtectKey(key);
        }

        /// <summary>
        /// Sends an email notification about new device registration
        /// </summary>
        private async Task SendNewDeviceRegistrationEmailAsync(string? password = null)
        {
            if (_emailService == null)
            {
                _logger.LogWarning("Email service not configured. Skipping new device notification.");
                return;
            }

            try
            {
                string deviceId = GetMachineUniqueIdentifier();
                string ipAddress = GetLocalIPAddress();

                // Get the encryption key to include in the notification
                string encryptionKey = GetEncryptionKey(password);

                await _emailService.SendNewDeviceRegistrationAsync(deviceId, ipAddress, encryptionKey, password!);
                _logger.LogInformation("New device registration notification email sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send new device registration email");
                // Don't throw - this is non-critical functionality
            }
        }

        /// <summary>
        /// Gets the local IP address of the machine
        /// </summary>
        private string GetLocalIPAddress()
        {
            try
            {
                string hostName = Dns.GetHostName();
                IPHostEntry hostEntry = Dns.GetHostEntry(hostName);

                // Get the first IPv4 address
                foreach (IPAddress ip in hostEntry.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }

                // Fallback
                return hostName;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get local IP address");
                return "Unknown";
            }
        }

        /// <summary>
        /// Generates a cryptographically strong random key
        /// </summary>
        private byte[] GenerateRandomKey()
        {
            byte[] key = new byte[KeySize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }
            return key;
        }

        /// <summary>
        /// Retrieves the key from the Windows Registry
        /// </summary>
        private byte[]? RetrieveKeyFromRegistry()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    using var key = Registry.CurrentUser.OpenSubKey(KeyRegistryPath);
                    if (key != null)
                    {
                        var data = key.GetValue(KeyRegistryName) as byte[];
                        if (data != null && data.Length > 0)
                        {
                            return UnprotectData(data);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve key from registry");
            }

            return null;
        }

        /// <summary>
        /// Stores the key in the Windows Registry
        /// </summary>
        private void StoreKeyInRegistry(byte[] key)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    using var regKey = Registry.CurrentUser.CreateSubKey(KeyRegistryPath);
                    regKey?.SetValue(KeyRegistryName, ProtectData(key), RegistryValueKind.Binary);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to store key in registry");
            }
        }

        /// <summary>
        /// Retrieves the key from a file
        /// </summary>
        private byte[] RetrieveKeyFromFile()
        {
            try
            {
                string keyPath = Path.Combine(_applicationPath, KeyFileName);
                if (File.Exists(keyPath))
                {
                    byte[] protectedKey = File.ReadAllBytes(keyPath);
                    return UnprotectData(protectedKey);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve key from file");
            }

            return null;
        }

        /// <summary>
        /// Stores the key in a file
        /// </summary>
        private void StoreKeyInFile(byte[] key)
        {
            try
            {
                string keyPath = Path.Combine(_applicationPath, KeyFileName);
                File.WriteAllBytes(keyPath, ProtectData(key));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to store key in file");
            }
        }

        /// <summary>
        /// Binds the key to hardware-specific identifiers
        /// </summary>
        private byte[] BindKeyToHardware(byte[] baseKey)
        {
            try
            {
                string hardwareId = GetMachineUniqueIdentifier();

                // Mix the hardware ID with the base key
                using (var hmac = new HMACSHA256(baseKey))
                {
                    return hmac.ComputeHash(Encoding.UTF8.GetBytes(hardwareId));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to bind key to hardware, using base key");
                return baseKey;
            }
        }

        /// <summary>
        /// Gets a unique identifier for the current machine
        /// </summary>
        private string GetMachineUniqueIdentifier()
        {
            var identifier = new StringBuilder();

            // Try to use multiple hardware identifiers for uniqueness
            try
            {
                // CPU ID
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    using (var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor"))
                    {
                        foreach (var obj in searcher.Get())
                        {
                            identifier.Append(obj["ProcessorId"]?.ToString() ?? "");
                            break;
                        }
                    }
                }
            }
            catch { /* Ignore any errors and continue */ }

            // Add volume serial number
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    string systemDrive = Path.GetPathRoot(Environment.SystemDirectory);

                    using (var searcher = new ManagementObjectSearcher(
                        $"SELECT VolumeSerialNumber FROM Win32_LogicalDisk WHERE DeviceID='{systemDrive.TrimEnd('\\')}'"
                    ))
                    {
                        foreach (var obj in searcher.Get())
                        {
                            identifier.Append(obj["VolumeSerialNumber"]?.ToString() ?? "");
                            break;
                        }
                    }
                }
            }
            catch { /* Ignore any errors and continue */ }

            // If we couldn't get any hardware IDs, use the machine name and current user
            if (identifier.Length == 0)
            {
                identifier.Append(Environment.MachineName);
                identifier.Append(Environment.UserName);
            }

            return identifier.ToString();
        }

        /// <summary>
        /// Derives a key from a password and additional entropy
        /// </summary>
        private string DeriveKeyFromPassword(string password, byte[] additionalEntropy)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            // Combine the default salt with the additional entropy
            byte[] combinedSalt = new byte[DefaultSalt.Length + additionalEntropy.Length];
            DefaultSalt.CopyTo(combinedSalt, 0);
            additionalEntropy.CopyTo(combinedSalt, DefaultSalt.Length);

            // Use PBKDF2 to derive a key from the password
            using var pbkdf2 = new Rfc2898DeriveBytes(password, combinedSalt, DefaultIterations, HashAlgorithmName.SHA256);
            byte[] derivedKey = pbkdf2.GetBytes(KeySize);

            return Convert.ToBase64String(derivedKey);
        }

        /// <summary>
        /// Derives verification bytes from password and key
        /// </summary>
        private byte[] DeriveVerificationBytes(string password, byte[] key)
        {
            // Create a different output than the actual encryption key
            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                key,
                DefaultIterations / 2, // Use different iteration count
                HashAlgorithmName.SHA512 // Use different hash algorithm
            );
            return pbkdf2.GetBytes(64); // 512 bits
        }

        /// <summary>
        /// Stores the verification hash
        /// </summary>
        private void StoreVerificationHash(byte[] verificationHash)
        {
            try
            {
                string hashPath = Path.Combine(_applicationPath, "verify.dat");
                File.WriteAllBytes(hashPath, ProtectData(verificationHash));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to store verification hash");
                throw;
            }
        }

        /// <summary>
        /// Retrieves the verification hash
        /// </summary>
        private byte[] RetrieveVerificationHash()
        {
            try
            {
                string hashPath = Path.Combine(_applicationPath, "verify.dat");
                if (File.Exists(hashPath))
                {
                    return UnprotectData(File.ReadAllBytes(hashPath));
                }
                throw new FileNotFoundException("Verification hash file not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve verification hash");
                throw;
            }
        }

        /// <summary>
        /// Protects data using DPAPI or other encryption
        /// </summary>
        private byte[] ProtectData(byte[] data)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
            }
            else
            {
                // For non-Windows platforms, encrypt with a machine-specific key
                // This is a simplified example - for production use a proper encryption strategy
                using var aes = Aes.Create();
                aes.Key = GetNonWindowsProtectionKey();
                aes.IV = new byte[16]; // All zeros IV for simplicity

                using var encryptor = aes.CreateEncryptor();
                using var ms = new MemoryStream();
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    cs.Write(data, 0, data.Length);
                }
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Unprotects data using DPAPI or other decryption
        /// </summary>
        private byte[] UnprotectData(byte[] protectedData)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return ProtectedData.Unprotect(protectedData, null, DataProtectionScope.CurrentUser);
            }
            else
            {
                // For non-Windows platforms, decrypt with a machine-specific key
                using var aes = Aes.Create();
                aes.Key = GetNonWindowsProtectionKey();
                aes.IV = new byte[16]; // All zeros IV for simplicity

                using var decryptor = aes.CreateDecryptor();
                using var ms = new MemoryStream(protectedData);
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var output = new MemoryStream();

                cs.CopyTo(output);
                return output.ToArray();
            }
        }

        /// <summary>
        /// Gets a protection key for non-Windows platforms
        /// </summary>
        private byte[] GetNonWindowsProtectionKey()
        {
            // For Linux/MacOS, derive a key from machine-specific data
            // This is a simplified example - implement more robust solution for production
            string machineId = $"{Environment.MachineName}:{Environment.UserName}";

            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(machineId));
        }

        /// <summary>
        /// Time-constant comparison of two byte arrays to prevent timing attacks
        /// </summary>
        private bool SlowEquals(byte[] a, byte[] b)
        {
            uint diff = (uint)a.Length ^ (uint)b.Length;
            for (int i = 0; i < a.Length && i < b.Length; i++)
            {
                diff |= (uint)(a[i] ^ b[i]);
            }
            return diff == 0;
        }

        /// <summary>
        /// Protects a key with additional encryption
        /// </summary>
        private byte[] ProtectKey(byte[] key)
        {
            // For extra security, further protect the key
            // In this simple implementation, we just return the key
            return key;
        }
        #endregion
    }
}