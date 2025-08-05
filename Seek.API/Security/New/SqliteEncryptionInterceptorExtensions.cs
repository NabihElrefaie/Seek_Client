using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Seek.Core.Security;
using Seek.API.Services.Interceptors;

namespace Seek.API.Security.New
{
    /// <summary>
    /// Extension methods for the SqliteEncryptionInterceptor
    /// </summary>
    public static class SqliteEncryptionInterceptorExtensions
    {
        /// <summary>
        /// Creates an enhanced SqliteEncryptionInterceptor that checks for verification
        /// </summary>
        public static SqliteEncryptionInterceptor CreateWithVerification(
            this IServiceProvider serviceProvider,
            string fallbackKey)
        {
            var keyManager = serviceProvider.GetRequiredService<SecureKeyManager>();
            var logger = serviceProvider.GetRequiredService<ILogger<SqliteEncryptionInterceptor>>();
            var verificationService = serviceProvider.GetRequiredService<VerificationService>();
            var userPassword = serviceProvider.GetRequiredService<IConfiguration>()["UserPassword"];

            // Create enhanced interceptor
            return new VerificationSqliteEncryptionInterceptor(
                keyManager,
                fallbackKey,
                logger,
                verificationService,
                userPassword);
        }
    }

    /// <summary>
    /// SqliteEncryptionInterceptor that checks for application verification
    /// </summary>
    public class VerificationSqliteEncryptionInterceptor : SqliteEncryptionInterceptor
    {
        private readonly VerificationService _verificationService;
        private readonly ILogger _logger;

        public VerificationSqliteEncryptionInterceptor(
            SecureKeyManager keyManager,
            string fallbackKey,
            ILogger<SqliteEncryptionInterceptor> logger,
            VerificationService verificationService,
            string userPassword = null)
            : base(GetEncryptionKey(keyManager, fallbackKey, logger, userPassword), logger)
        {
            _verificationService = verificationService;
            _logger = logger;
        }

        protected  void ApplyEncryption(System.Data.Common.DbConnection connection)
        {
            // Check if application is verified before allowing connection
            if (!_verificationService.IsVerificationCompleted())
            {
                _logger.LogWarning("Database access attempted without application verification");
                throw new UnauthorizedAccessException("Application must be verified before accessing the database");
            }

            // If verified, proceed with normal encryption
            ApplyEncryption(connection);
        }

        private static string GetEncryptionKey(
            SecureKeyManager keyManager,
            string fallbackKey,
            ILogger logger,
            string userPassword = null)
        {
            try
            {
                // Try to get key from secure manager
                var key = keyManager.GetEncryptionKey(userPassword);
                logger.LogInformation("Using securely stored encryption key");
                return key;
            }
            catch (Exception ex)
            {
                // Fall back to provided key if secure manager fails
                logger.LogWarning(ex, "Failed to get key from secure manager, using fallback key");
                return fallbackKey;
            }
        }
    }
}