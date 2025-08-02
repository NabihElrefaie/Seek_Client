using Seek.Core.IRepositories;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Seek.EF.Repositories
{
    public class Repo_Database_Security : IRepo_Database_Security
    {
        private readonly ILogger<Repo_Database_Security> _logger;
        private readonly IConfiguration _configuration; // Add IConfiguration dependency

        public Repo_Database_Security(ILogger<Repo_Database_Security> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

        }

        private async Task<bool> IsDatabaseEncrypted(string dbPath)
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    await connection.OpenAsync();

                    // First, try to read from sqlite_master without a key
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT name FROM sqlite_master LIMIT 1;";
                        try
                        {
                            var result = await command.ExecuteScalarAsync();
                            // If we get a result without providing a key, it's unencrypted
                            return false;
                        }
                        catch (SqliteException ex) when (ex.SqliteErrorCode == 26 /* SQLITE_NOTADB */)
                        {
                            // This error typically means the database is encrypted
                            return true;
                        }
                        catch (SqliteException ex) when (ex.Message.Contains("file is encrypted", StringComparison.OrdinalIgnoreCase))
                        {
                            // Another common error message for encrypted databases
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking database encryption status");
                // If we can't even open the file, assume it's encrypted
                return true;
            }
        }
        
        public async Task<(bool Success, string Message)> DecryptDatabaseAsync(string encryptedDbPath, string plainDbPath, string encryptionKey)
        {
            // Retrieve the correct encryption key from the configuration
            var configuredEncryptionKey = _configuration["EncryptionKey"];

            // Compare the provided key with the key in the configuration
            if (encryptionKey != configuredEncryptionKey)
            {
                _logger.LogWarning("Incorrect encryption key provided.");
                return (false, "Incorrect encryption key.");
            }

            // Check if the database is encrypted
            if (!await IsDatabaseEncrypted(encryptedDbPath))
            {
                _logger.LogWarning("Database is not encrypted, skipping decryption.");
                return (false, "Database is not encrypted, skipping decryption.");
            }

            try
            {
                using (var connection = new SqliteConnection($"Data Source={encryptedDbPath}"))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        // Set the key to decrypt
                        command.CommandText = $"PRAGMA key = '{encryptionKey}';";
                        await command.ExecuteNonQueryAsync();

                        // Verify decryption by executing a simple command
                        command.CommandText = "SELECT name FROM sqlite_master LIMIT 1;";
                        var result = await command.ExecuteScalarAsync();

                        // If the query fails or returns no results, it indicates a decryption problem
                        if (result == null || result == DBNull.Value)
                        {
                            _logger.LogWarning("Failed to decrypt the database with the provided key.");
                            return (false, "Incorrect password or the database could not be decrypted.");
                        }

                        // Attach plaintext database
                        command.CommandText = $"ATTACH DATABASE '{plainDbPath}' AS plaintext KEY '';";
                        await command.ExecuteNonQueryAsync();

                        // Export decrypted data
                        command.CommandText = "SELECT sqlcipher_export('plaintext');";
                        await command.ExecuteNonQueryAsync();

                        // Detach plaintext database
                        command.CommandText = "DETACH DATABASE plaintext;";
                        await command.ExecuteNonQueryAsync();

                        await connection.CloseAsync();
                    }
                }

                // Backup decrypted database into the tempdata folder
                var tempDataDir = Path.Combine(Path.GetDirectoryName(encryptedDbPath), "Decrypt");
                Directory.CreateDirectory(tempDataDir);  // Ensure the tempdata folder exists
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupPath = Path.Combine(tempDataDir, $"decrypted_{timestamp}.db");

                // Move the decrypted database file to the tempdata folder
                File.Move(plainDbPath, backupPath);

                _logger.LogInformation($"Database decrypted successfully. Backup at: {backupPath}");
                return (true, "Database decrypted successfully.");
            }
            catch (SqliteException sqlEx)
            {
                // Handle database-specific errors
                _logger.LogError(sqlEx, "SQL error occurred while decrypting database.");
                return (false, "An error occurred while decrypting the database. Please check the password and try again.");
            }
            catch (Exception ex)
            {
                // General exception handling
                _logger.LogError(ex, "Failed to decrypt database");
                return (false, "An unexpected error occurred while decrypting the database.");
            }
        }
        public async Task<(bool Success, string Message)> EncryptDatabaseAsync(string plainDbPath, string encryptedDbPath, string encryptionKey)
        {
            // Check if database is already encrypted
            bool isEncrypted = await IsDatabaseEncrypted(plainDbPath);
            if (isEncrypted)
            {
                _logger.LogWarning("Database is already encrypted.");
                return (false, "Database is already encrypted");
            }

            try
            {
                using (var connection = new SqliteConnection($"Data Source={plainDbPath}"))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        // Set the key to verify if unencrypted
                        command.CommandText = "PRAGMA cipher_version;";
                        await command.ExecuteScalarAsync();

                        // Proceed with encryption
                        command.CommandText = $"ATTACH DATABASE '{encryptedDbPath}' AS encrypted KEY '{encryptionKey}';";
                        await command.ExecuteNonQueryAsync();

                        command.CommandText = "SELECT sqlcipher_export('encrypted');";
                        await command.ExecuteNonQueryAsync();

                        command.CommandText = "DETACH DATABASE encrypted;";
                        await command.ExecuteNonQueryAsync();

                        await connection.CloseAsync();
                    }
                }

                // Backup encrypted database
                var tempDataDir = Path.Combine(Path.GetDirectoryName(plainDbPath), "../Encrypted");
                Directory.CreateDirectory(tempDataDir);
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupPath = Path.Combine(tempDataDir, $"encrypted_{timestamp}.db");
                File.Move(encryptedDbPath, backupPath);

                _logger.LogInformation($"Database encrypted successfully. Backup at: {backupPath}");
                return (true, "Database encrypted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to encrypt database");
                return (false, "Failed to encrypt database");
            }
        }
    }
}