using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Seek.Core.Security;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Seek.API.Services.Interceptors
{
    /// <summary>
    /// Interceptor that applies encryption settings to SQLite connections
    /// </summary>
    public class SqliteEncryptionInterceptor : DbConnectionInterceptor
    {
        private readonly string _encryptionKey;
        private readonly ILogger<SqliteEncryptionInterceptor> _logger;
        private readonly bool _allowKeyRefresh;

        /// <summary>
        /// Creates a new instance of SqliteEncryptionInterceptor
        /// </summary>
        /// <param name="encryptionKey">The encryption key for the database</param>
        /// <param name="logger">Logger instance</param>
        /// <param name="allowKeyRefresh">Whether to allow refreshing the key from secure storage on connection open (default: false)</param>
        public SqliteEncryptionInterceptor(string encryptionKey, ILogger<SqliteEncryptionInterceptor> logger, bool allowKeyRefresh = false)
        {
            _encryptionKey = encryptionKey ?? throw new ArgumentNullException(nameof(encryptionKey));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _allowKeyRefresh = allowKeyRefresh;
        }

        public override InterceptionResult ConnectionOpening(DbConnection connection, ConnectionEventData eventData, InterceptionResult result)
        {
            ApplyEncryption(connection);
            return result;
        }

        public override async ValueTask<InterceptionResult> ConnectionOpeningAsync(DbConnection connection, ConnectionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = default)
        {
            ApplyEncryption(connection);
            return await ValueTask.FromResult(result);
        }

        private void ApplyEncryption(DbConnection connection)
        {
            if (connection is SqliteConnection sqliteConnection)
            {
                try
                {
                    // Check if we need to modify the connection string
                    var builder = new SqliteConnectionStringBuilder(sqliteConnection.ConnectionString);

                    // Store connection open time to handle key refresh
                    var encryptionKey = _encryptionKey;
                    var connectionOpenTime = DateTime.UtcNow;

                    // Apply encryption key using PRAGMA after opening
                    sqliteConnection.StateChange += (sender, args) =>
                    {
                        if (args.CurrentState == System.Data.ConnectionState.Open)
                        {
                            // Check if we need to refresh the key (only applicable if allowed and connection was closed)
                            if (_allowKeyRefresh && args.OriginalState == System.Data.ConnectionState.Closed)
                            {
                                // For production, implement a secure key refresh mechanism here
                                // This is just a placeholder for the concept
                                _logger.LogDebug("Connection opened at {time}, using current encryption key", connectionOpenTime);
                            }

                            // Apply encryption key
                            using var command = sqliteConnection.CreateCommand();
                            command.CommandText = $"PRAGMA key = '{encryptionKey}';";
                            try
                            {
                                command.ExecuteNonQuery();
                                _logger.LogDebug("Successfully applied encryption key to database connection");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to apply encryption key to SQLite connection");
                            }
                        }
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error configuring SQLite encryption");
                }
            }
        }
    }

    /// <summary>
    /// Factory for creating SQLite encryption interceptors with appropriate key management
    /// </summary>
    public static class SqliteEncryptionInterceptorFactory
    {
        /// <summary>
        /// Creates an interceptor with a secure key from the key manager or falls back to a provided key
        /// </summary>
        public static SqliteEncryptionInterceptor Create(
            SecureKeyManager keyManager,
            string fallbackKey,
            ILogger<SqliteEncryptionInterceptor> logger,
            string userPassword = null)
        {
            string key;
            try
            {
                // Try to get key from secure manager
                key = keyManager.GetEncryptionKey(userPassword);
                logger.LogInformation("Using securely stored encryption key");
            }
            catch (Exception ex)
            {
                // Fall back to provided key if secure manager fails
                logger.LogWarning(ex, "Failed to get key from secure manager, using fallback key");
                key = fallbackKey;
            }

            return new SqliteEncryptionInterceptor(key, logger, true);
        }
    }
}