using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Seek.Core.Helper_Classes;
using Seek.Core.IRepositories;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Seek.EF.Repositories
{
    public class Repo_Database_Security : IRepo_Database_Security
    {
        private readonly ILogger<Repo_Database_Security> _logger;
        private readonly MaintenanceService _maintenanceService;

        // Constructor: Inject MaintenanceService
        public Repo_Database_Security(ILogger<Repo_Database_Security> logger, MaintenanceService maintenanceService)
        {
            _logger = logger;
            _maintenanceService = maintenanceService;
        }

        public async Task<bool> DecryptDatabaseAsync(string encryptedDbPath, string plainDbPath, string encryptionKey)
        {
            try
            {
                // Step 1: Enable maintenance mode before the operation
                _maintenanceService.EnableMaintenance();
                // Step 2: Ensure that no other process is using the database
                await EnsureFileIsNotLocked(encryptedDbPath);

                if (File.Exists(plainDbPath))
                    File.Delete(plainDbPath);

                using var connection = new SqliteConnection($"Data Source={encryptedDbPath}");
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"PRAGMA key = '{encryptionKey}';";
                    await command.ExecuteNonQueryAsync();

                    command.CommandText = $"ATTACH DATABASE '{plainDbPath}' AS plaintext KEY '';";
                    await command.ExecuteNonQueryAsync();

                    command.CommandText = "SELECT sqlcipher_export('plaintext');";
                    await command.ExecuteNonQueryAsync();

                    command.CommandText = "DETACH DATABASE plaintext;";
                    await command.ExecuteNonQueryAsync();
                }

                await connection.CloseAsync();

                // Replace encrypted file with plain
                File.Delete(encryptedDbPath);
                File.Move(plainDbPath, encryptedDbPath);

                _logger.LogInformation("SQLite: Database decrypted successfully.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SQLite: Failed to decrypt database.");
                return false;
            }
            finally
            {
                // Step 3: Disable maintenance mode after the operation
                _maintenanceService.DisableMaintenance();
            }
        }

        public async Task<bool> EncryptDatabaseAsync(string plainDbPath, string encryptedDbPath, string encryptionKey)
        {
            try
            {
                // Step 1: Enable maintenance mode before the operation
                _maintenanceService.EnableMaintenance();

                // Step 2: Ensure that no other process is using the database
                await EnsureFileIsNotLocked(plainDbPath);

                if (File.Exists(encryptedDbPath))
                    File.Delete(encryptedDbPath);

                using var connection = new SqliteConnection($"Data Source={plainDbPath}");
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"ATTACH DATABASE '{encryptedDbPath}' AS encrypted KEY '{encryptionKey}';";
                    await command.ExecuteNonQueryAsync();

                    command.CommandText = "SELECT sqlcipher_export('encrypted');";
                    await command.ExecuteNonQueryAsync();

                    command.CommandText = "DETACH DATABASE encrypted;";
                    await command.ExecuteNonQueryAsync();
                }

                await connection.CloseAsync();

                // Replace plain file with encrypted one
                File.Delete(plainDbPath);
                File.Move(encryptedDbPath, plainDbPath);

                _logger.LogInformation("SQLite: Database encrypted successfully.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SQLite: Failed to encrypt database.");
                return false;
            }
            finally
            {
                // Step 3: Disable maintenance mode after the operation
                _maintenanceService.DisableMaintenance();
            }
        }

        /// <summary>
        /// Waits for file to be unlocked before proceeding.
        /// </summary>
        private async Task EnsureFileIsNotLocked(string filePath, int retries = 10, int delayMs = 300)
        {
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    using (File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { }
                    return;
                }
                catch (IOException)
                {
                    _logger.LogWarning($"SQLite: File '{filePath}' is locked. Retrying ({i + 1}/{retries})...");
                    await Task.Delay(delayMs);
                }
            }

            throw new IOException($"SQLite: Timeout waiting for file '{filePath}' to be unlocked.");
        }
    }
}
