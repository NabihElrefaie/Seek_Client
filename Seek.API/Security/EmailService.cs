using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using com.sun.tools.javac.util;
using Humanizer;
using java.security;
using javax.swing.text.html;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using NuGet.Configuration;
using static javax.xml.crypto.KeySelector;
using static QRCoder.PayloadGenerator.SwissQrCode;

namespace Seek.API.Security
{
    /// <summary>
    /// Configuration settings for email service
    /// </summary>
    public class EmailSettings
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool UseSsl { get; set; }
        public string FromEmail { get; set; }
        public string AdminEmail { get; set; }
    }

    /// <summary>
    /// Service for sending email notifications related to security events
    /// </summary>
    public class EmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings = null, ILogger<EmailService> logger = null)
        {
            _settings = settings?.Value;
            _logger = logger;
        }

        /// <summary>
        /// Sends an email notification about a new device registration
        /// </summary>
        /// <param name="deviceId">Unique identifier of the registered device</param>
        /// <param name="ipAddress">IP address of the registered device</param>
        /// <param name="encryptionKey">The encryption key used for the database</param>
        /// <param name="password">Optional: indication if a password was used (not the actual password)</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task SendNewDeviceRegistrationAsync(string deviceId, string ipAddress, string encryptionKey, string password = null)
        {
            if (_settings == null)
            {
                _logger?.LogWarning("Email settings not configured. Cannot send new device registration notification.");
                return;
            }

            try
            {
                string subject = "Security Alert: New Device Registration";
                string body = BuildNewDeviceRegistrationEmail(deviceId, ipAddress, encryptionKey, !string.IsNullOrEmpty(password));

                await SendEmailAsync(_settings.AdminEmail, subject, body);
                _logger?.LogInformation("New device registration notification email sent successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to send new device registration email");
                // Don't throw - this is non-critical functionality
            }
        }

        /// <summary>
        /// Builds the email body for a new device registration notification
        /// </summary>
        private string BuildNewDeviceRegistrationEmail(string deviceId, string ipAddress, string encryptionKey, bool passwordProtected)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; border: 1px solid #ddd; border-radius: 5px; }}
        .header {{ background-color: #f8f8f8; padding: 20px; border-bottom: 1px solid #ddd; }}
        .content {{ padding: 20px; }}
        .alert {{ color: #721c24; background-color: #f8d7da; padding: 10px; border-radius: 5px; margin-bottom: 20px; }}
        .footer {{ background-color: #f8f8f8; padding: 20px; border-top: 1px solid #ddd; font-size: 12px; text-align: center; }}
        table {{ width: 100%; border-collapse: collapse; margin-bottom: 20px; }}
        th, td {{ padding: 10px; text-align: left; border-bottom: 1px solid #ddd; }}
        th {{ background-color: #f2f2f2; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>Security Alert: New Device Registration</h2>
        </div>
        <div class='content'>
            <div class='alert'>
                A new device has been registered with your Seek application.
            </div>
            
            <p>A new device has been registered to access your encrypted data. If this was you, no action is needed. If you did not register a new device, please contact your system administrator immediately.</p>
            
            <table>
                <tr>
                    <th>Information</th>
                    <th>Details</th>
                </tr>
                <tr>
                    <td>Device ID:</td>
                    <td>{deviceId}</td>
                </tr>
                <tr>
                    <td>IP Address:</td>
                    <td>{ipAddress}</td>
                </tr>
                <tr>
                    <td>Time (UTC):</td>
                    <td>{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}</td>
                </tr>
                <tr>
                    <td>Password Protected:</td>
                    <td>{(passwordProtected ? "Yes" : "No")}</td>
                </tr>
               <tr>
                <td>Encryption Key:</td>
                <td><code style=""background-color: #f8f8f8; padding: 5px; border: 1px solid #ddd; border-radius: 3px;"">{encryptionKey}</code></td>
            </tr>
            </table>


            <div style=""background-color: #fff3cd; border: 1px solid #ffeeba; color: #856404; padding: 15px; margin: 20px 0; border-radius: 5px;"">
                <strong>Important:</strong> The encryption key above is required for database decryption. Please store it securely for technical support purposes.
            </div>
            <p><strong> What should I do?</strong></p>
            <p> If you authorized this device, no action is required.If you did not authorize this device, please:</p>
            <ol>
                <li> Change your password immediately </li>
                <li> Contact your system administrator </li>
                <li> Review your security settings </li>
            </ol>
        </div>
        <div class='footer'>
            This is an automated message from your Seek application.Please do not reply to this email.
        </div>
    </div>
</body>
</html>";
        }

        /// <summary>
        /// Sends an email with the specified subject and body to the recipient
        /// </summary>
        private async Task SendEmailAsync(string recipient, string subject, string body)
        {
            if (_settings == null)
            {
                throw new InvalidOperationException("Email settings not configured");
            }

            using var client = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = _settings.UseSsl
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(recipient);

            await client.SendMailAsync(message);
        }
    }
}