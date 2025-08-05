using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Seek.Core.Security;

namespace Seek.API.Security.New
{
    /// <summary>
    /// Extension methods for the EmailService class
    /// </summary>
    public static class EmailServiceExtensions
    {
        /// <summary>
        /// Sends a verification code email to the specified recipient
        /// </summary>
        /// <param name="emailService">The email service instance</param>
        /// <param name="recipient">Email recipient</param>
        /// <param name="verificationCode">The verification code to send</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public static async Task SendVerificationCodeAsync(this EmailService emailService, string recipient, string verificationCode)
        {
            if (emailService == null)
            {
                throw new ArgumentNullException(nameof(emailService));
            }

            string subject = "Seek Application Verification Code";
            string body = BuildVerificationCodeEmail(verificationCode);

            try
            {
                // Use reflection to call the private SendEmailAsync method
                var sendEmailMethod = typeof(EmailService).GetMethod("SendEmailAsync",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (sendEmailMethod == null)
                {
                    throw new InvalidOperationException("SendEmailAsync method not found in EmailService class");
                }

                var task = (Task)sendEmailMethod.Invoke(emailService, new object[] { recipient, subject, body });
                await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Log the error if we have a logger available
                var loggerField = typeof(EmailService).GetField("_logger",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (loggerField?.GetValue(emailService) is ILogger logger)
                {
                    logger.LogError(ex, "Failed to send verification code email");
                }

                throw new Exception("Failed to send verification code email", ex);
            }
        }

        /// <summary>
        /// Builds the email body for a verification code notification
        /// </summary>
        private static string BuildVerificationCodeEmail(string verificationCode)
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
        .code-box {{ background-color: #f2f2f2; padding: 15px; border: 1px solid #ddd; border-radius: 5px; text-align: center; font-size: 24px; font-weight: bold; letter-spacing: 5px; margin: 20px 0; }}
        .footer {{ background-color: #f8f8f8; padding: 20px; border-top: 1px solid #ddd; font-size: 12px; text-align: center; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>Seek Application Verification</h2>
        </div>
        <div class='content'>
            <p>Thank you for installing the Seek application. To verify and activate your installation, please use the verification code below:</p>
            
            <div class='code-box'>
                {verificationCode}
            </div>
            
            <p>Enter this code in the verification prompt in your Seek application to complete the activation process.</p>
            
            <p><strong>Important:</strong> This verification code is required to activate your application. If you did not request this code, please ignore this email.</p>
        </div>
        <div class='footer'>
            This is an automated message from your Seek application. Please do not reply to this email.
            <br>
            Verification code expires in 24 hours.
        </div>
    </div>
</body>
</html>";
        }
    }
}