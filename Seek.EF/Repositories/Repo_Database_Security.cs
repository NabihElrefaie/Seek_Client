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
        private readonly IConfiguration _configuration;
        public Repo_Database_Security(ILogger<Repo_Database_Security> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

        }
        public async Task<(bool Success, string Message)> DecryptDatabaseAsync(string encryptedDbPath, string plainDbPath, string encryptionKey)
        {
            // Retrieve encryption key from configuration
            var configuredEncryptionKey = _configuration["EncryptionKey"];

            if (encryptionKey != configuredEncryptionKey)
            {
                _logger.LogWarning("SQLite : Incorrect encryption key provided.");
                return (false, "SQLite : Incorrect encryption key.");
            }
            try
            {
                using (var connection = new SqliteConnection($"Data Source={encryptedDbPath}"))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        // Set the key for decryption
                        command.CommandText = $"PRAGMA key = '{encryptionKey}';";
                        await command.ExecuteNonQueryAsync();

                        // Verify decryption by executing a simple query
                        command.CommandText = "SELECT name FROM sqlite_master LIMIT 1;";
                        var result = await command.ExecuteScalarAsync();

                        if (result == null || result == DBNull.Value)
                        {
                            _logger.LogWarning("SQLite : Failed to decrypt the database with the provided key.");
                            return (false, "SQLite : Incorrect password or the database could not be decrypted.");
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
                // Backup the decrypted database
                var tempDataDir = Path.Combine(Path.GetDirectoryName(encryptedDbPath), "Decrypt");
                Directory.CreateDirectory(tempDataDir);
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupPath = Path.Combine(tempDataDir, $"decrypted_{timestamp}.db");
                File.Move(plainDbPath, backupPath);

                _logger.LogInformation("SQLite : Database decrypted successfully.");
                return (true, "SQLite : Database decrypted successfully.");
            }
            catch (SqliteException sqlEx)
            {
                _logger.LogError(sqlEx, "SQLite : SQL error occurred during decryption");
                return (false, "SQLite : An error occurred while decrypting the database. Please check the password and try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SQLite : Unexpected error during decryption");
                return (false, "SQLite : An unexpected error occurred during decryption.");
            }
        }
        public async Task<(bool Success, string Message)> EncryptDatabaseAsync(string plainDbPath, string encryptedDbPath, string encryptionKey)
        {
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

                _logger.LogInformation($"SQLite : Database encrypted successfully");
                return (true, "SQLite : Database encrypted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SQLite : Failed to encrypt database");
                return (false, "SQLite : Failed to encrypt database");
            }
        }
    }
}