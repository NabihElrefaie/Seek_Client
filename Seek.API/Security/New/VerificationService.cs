using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Seek.API.Security.New
{
    /// <summary>
    /// Service that manages application verification through email codes
    /// </summary>
    public class VerificationService
    {
        private readonly string _workingDirectory;
        private readonly ILogger<VerificationService> _logger;
        private readonly string _verificationFilePath;
        private const int VerificationCodeLength = 6;
        private const int ExpirationMinutes = 30;

        // Storage structure for verification data
        private class VerificationData
        {
            public string CodeHash { get; set; }
            public DateTime ExpiresAt { get; set; }
            public bool IsVerified { get; set; }
            public DateTime? VerifiedAt { get; set; }
        }

        public VerificationService(string workingDirectory, ILogger<VerificationService> logger)
        {
            _workingDirectory = workingDirectory;
            _logger = logger;
            _verificationFilePath = Path.Combine(_workingDirectory, "verification_status.json");
            EnsureVerificationStatusFile();
        }

        /// <summary>
        /// Generates a new verification code and stores its hash
        /// </summary>
        /// <returns>The generated verification code</returns>
        public async Task<string> GenerateVerificationCodeAsync()
        {
            try
            {
                // Generate a random code
                string code = GenerateRandomCode(VerificationCodeLength);

                // Store the hash of the code
                var verificationData = new VerificationData
                {
                    CodeHash = HashVerificationCode(code),
                    ExpiresAt = DateTime.UtcNow.AddMinutes(ExpirationMinutes),
                    IsVerified = false,
                    VerifiedAt = null
                };

                await SaveVerificationDataAsync(verificationData);
                _logger.LogInformation("Verification code generated successfully");

                return code;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate verification code");
                throw;
            }
        }

        /// <summary>
        /// Verifies if the provided code matches the stored hash
        /// </summary>
        /// <param name="code">The verification code to verify</param>
        /// <returns>True if verification was successful, false otherwise</returns>
        public async Task<bool> VerifyCodeAsync(string code)
        {
            try
            {
                var verificationData = await LoadVerificationDataAsync();

                // If already verified, return true
                if (verificationData.IsVerified)
                {
                    return true;
                }

                // Check if expired
                if (DateTime.UtcNow > verificationData.ExpiresAt)
                {
                    _logger.LogWarning("Verification code expired");
                    return false;
                }

                // Verify the code
                string codeHash = HashVerificationCode(code);
                bool isValid = codeHash == verificationData.CodeHash;

                if (isValid)
                {
                    // Update verification status
                    verificationData.IsVerified = true;
                    verificationData.VerifiedAt = DateTime.UtcNow;
                    await SaveVerificationDataAsync(verificationData);
                    _logger.LogInformation("Application verification successful");
                }
                else
                {
                    _logger.LogWarning("Invalid verification code provided");
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to verify code");
                throw;
            }
        }

        /// <summary>
        /// Checks if the application has been verified
        /// </summary>
        /// <returns>True if the application is verified, false otherwise</returns>
        public bool IsVerificationCompleted()
        {
            try
            {
                var verificationData = LoadVerificationDataAsync().GetAwaiter().GetResult();
                return verificationData.IsVerified;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check verification status");
                return false; // Default to not verified on error
            }
        }

        /// <summary>
        /// Gets the verification status details
        /// </summary>
        /// <returns>A tuple containing verification status and when it was verified (if applicable)</returns>
        public async Task<(bool IsVerified, DateTime? VerifiedAt)> GetVerificationStatusAsync()
        {
            try
            {
                var verificationData = await LoadVerificationDataAsync();
                return (verificationData.IsVerified, verificationData.VerifiedAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get verification status");
                return (false, null);
            }
        }

        /// <summary>
        /// Resets the verification status, requiring a new verification
        /// </summary>
        /// <returns>True if successfully reset, false otherwise</returns>
        public async Task<bool> ResetVerificationAsync()
        {
            try
            {
                var verificationData = await LoadVerificationDataAsync();
                verificationData.IsVerified = false;
                verificationData.VerifiedAt = null;
                verificationData.CodeHash = null;
                verificationData.ExpiresAt = DateTime.MinValue;
                await SaveVerificationDataAsync(verificationData);
                _logger.LogInformation("Verification status reset successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reset verification status");
                return false;
            }
        }

        #region Private Helper Methods

        private void EnsureVerificationStatusFile()
        {
            if (!File.Exists(_verificationFilePath))
            {
                var initialData = new VerificationData
                {
                    CodeHash = null,
                    ExpiresAt = DateTime.MinValue,
                    IsVerified = false,
                    VerifiedAt = null
                };

                string jsonString = JsonSerializer.Serialize(initialData);
                File.WriteAllText(_verificationFilePath, jsonString);
                _logger.LogInformation("Created new verification status file");
            }
        }

        private async Task<VerificationData> LoadVerificationDataAsync()
        {
            EnsureVerificationStatusFile();
            string jsonString = await File.ReadAllTextAsync(_verificationFilePath);
            return JsonSerializer.Deserialize<VerificationData>(jsonString);
        }

        private async Task SaveVerificationDataAsync(VerificationData data)
        {
            string jsonString = JsonSerializer.Serialize(data);
            await File.WriteAllTextAsync(_verificationFilePath, jsonString);
        }

        private string GenerateRandomCode(int length)
        {
            // Generate a numeric code of specified length
            StringBuilder code = new StringBuilder();
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] data = new byte[4];
                for (int i = 0; i < length; i++)
                {
                    rng.GetBytes(data);
                    int value = BitConverter.ToInt32(data, 0);
                    code.Append(Math.Abs(value % 10)); // Get a digit (0-9)
                }
            }
            return code.ToString();
        }

        private string HashVerificationCode(string code)
        {
            if (string.IsNullOrEmpty(code))
                return null;

            using (var sha = SHA256.Create())
            {
                byte[] textData = Encoding.UTF8.GetBytes(code);
                byte[] hash = sha.ComputeHash(textData);
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }

        #endregion
    }
}
