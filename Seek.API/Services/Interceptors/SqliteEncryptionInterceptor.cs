using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

public class SqliteEncryptionInterceptor : DbConnectionInterceptor
{
    private readonly string _encryptionKey;
    private readonly ILogger<SqliteEncryptionInterceptor> _logger;
    private bool _isKeyApplied = false;

    public SqliteEncryptionInterceptor(string encryptionKey, ILogger<SqliteEncryptionInterceptor> logger)
    {
        _encryptionKey = encryptionKey;
        _logger = logger;
    }

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        if (!_isKeyApplied)
        {
            try
            {
                SetEncryptionKey(connection);
                _logger.LogInformation("SQLite : encryption key applied successfully.");
                _isKeyApplied = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SQLite : Failed to apply SQLite encryption key.");
                throw;
            }
        }
    }

    public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        if (!_isKeyApplied)
        {
            try
            {
                SetEncryptionKey(connection);
                _logger.LogInformation("SQLite :  encryption key applied successfully (async).");
                _isKeyApplied = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SQLite : Failed to apply SQLite encryption key (async).");
                throw;
            }
        }
    }

    private void SetEncryptionKey(DbConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA key = '{_encryptionKey}';";
        command.ExecuteNonQuery();
    }
}