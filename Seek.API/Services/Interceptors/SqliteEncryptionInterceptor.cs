using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Seek.API
{
    public class SqliteEncryptionInterceptor : DbConnectionInterceptor
    {
        private readonly string _encryptionKey;
        private readonly ILogger<SqliteEncryptionInterceptor> _logger;

        public SqliteEncryptionInterceptor(string encryptionKey, ILogger<SqliteEncryptionInterceptor> logger)
        {
            _encryptionKey = encryptionKey ?? throw new ArgumentNullException(nameof(encryptionKey));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                    // Checking the connection string
                    var builder = new SqliteConnectionStringBuilder(sqliteConnection.ConnectionString);

                    // Apply encryption key using PRAGMA after opening
                    sqliteConnection.StateChange += (sender, args) =>
                    {
                        if (args.CurrentState == System.Data.ConnectionState.Open)
                        {
                            using var command = sqliteConnection.CreateCommand();
                            command.CommandText = $"PRAGMA key = '{_encryptionKey}';";
                            try
                            {
                                command.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to apply encryption key");
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
}