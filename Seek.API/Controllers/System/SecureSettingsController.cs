using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Seek.API.Security;
using Seek.API.Security.New;
using Seek.Core.Dtos.Settings;
using Seek.Core.Security;

namespace Seek.API.Controllers.System
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class SecureSettingsController : ControllerBase
    {
        private readonly ILogger<SecureSettingsController> _logger;
        private readonly SecureSettingsManager _secureSettingsManager;
        private readonly VerificationService _verificationService;

        public SecureSettingsController(
            ILogger<SecureSettingsController> logger,
            SecureSettingsManager secureSettingsManager,
            VerificationService verificationService)
        {
            _logger = logger;
            _secureSettingsManager = secureSettingsManager;
            _verificationService = verificationService;
        }

        /// <summary>
        /// Updates secure email settings
        /// </summary>
        [HttpPut("email")]
        public IActionResult UpdateEmailSettings([FromBody] EmailSettings emailSettings)
        {
            try
            {
                // Ensure application is verified before allowing settings changes
                if (!_verificationService.IsVerificationCompleted())
                {
                    return Forbid("Application must be verified before updating settings");
                }

                // Validate settings
                if (emailSettings == null ||
                    string.IsNullOrEmpty(emailSettings.SmtpServer) ||
                    string.IsNullOrEmpty(emailSettings.Username) ||
                    string.IsNullOrEmpty(emailSettings.Password) ||
                    string.IsNullOrEmpty(emailSettings.FromEmail) ||
                    string.IsNullOrEmpty(emailSettings.AdminEmail))
                {
                    return BadRequest(new { message = "All email settings fields are required" });
                }

                // Save to secure storage
                bool success = _secureSettingsManager.SaveSecureEmailSettings(emailSettings);

                if (success)
                {
                    _logger.LogInformation("Email settings updated successfully");
                    return Ok(new { message = "Email settings updated successfully" });
                }
                else
                {
                    _logger.LogError("Failed to save email settings");
                    return StatusCode(500, "Failed to save email settings");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating email settings");
                return StatusCode(500, "An error occurred while updating email settings");
            }
        }

        /// <summary>
        /// Gets secure email settings (without password)
        /// </summary>
        [HttpGet("email")]
        public IActionResult GetEmailSettings()
        {
            try
            {
                // Ensure application is verified before allowing access to settings
                if (!_verificationService.IsVerificationCompleted())
                {
                    return Forbid("Application must be verified before accessing settings");
                }

                // Get settings
                var settings = _secureSettingsManager.GetSecureEmailSettings();

                if (settings == null)
                {
                    return NotFound(new { message = "Email settings not found" });
                }

                // Don't return the password
                return Ok(new
                {
                    smtpServer = settings.SmtpServer,
                    smtpPort = settings.SmtpPort,
                    username = settings.Username,
                    useSsl = settings.UseSsl,
                    fromEmail = settings.FromEmail,
                    adminEmail = settings.AdminEmail,
                    // Password intentionally omitted for security
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving email settings");
                return StatusCode(500, "An error occurred while retrieving email settings");
            }
        }
    }
}