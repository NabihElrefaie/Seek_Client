using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Seek.API.Security;
using Seek.API.Security.New;
using Seek.Core.Security;

namespace Seek.API.Controllers.System
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class VerificationController : ControllerBase
    {
        private readonly ILogger<VerificationController> _logger;
        private readonly VerificationService _verificationService;
        private readonly EmailService _emailService;
        private readonly SecureSettingsManager _secureSettingsManager;

        public VerificationController(
            ILogger<VerificationController> logger,
            VerificationService verificationService,
            EmailService emailService,
            SecureSettingsManager secureSettingsManager)
        {
            _logger = logger;
            _verificationService = verificationService;
            _emailService = emailService;
            _secureSettingsManager = secureSettingsManager;
        }

        /// <summary>
        /// Checks if the application is verified
        /// </summary>
        [HttpGet("status")]
        public async Task<IActionResult> GetVerificationStatus()
        {
            try
            {
                var (isVerified, verifiedAt) = await _verificationService.GetVerificationStatusAsync();
                return Ok(new { isVerified, verifiedAt });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking verification status");
                return StatusCode(500, "An error occurred while checking verification status");
            }
        }

        /// <summary>
        /// Sends a verification code to the admin email
        /// </summary>
        [HttpPost("send-code")]
        public async Task<IActionResult> SendVerificationCode()
        {
            try
            {
                // Check if the app is already verified
                if (_verificationService.IsVerificationCompleted())
                {
                    return BadRequest(new { message = "Application is already verified" });
                }

                // Generate a new verification code
                string code = await _verificationService.GenerateVerificationCodeAsync();

                // Get admin email from secure settings
                var emailSettings = _secureSettingsManager.GetSecureEmailSettings();
                string adminEmail = emailSettings.AdminEmail;

                if (string.IsNullOrEmpty(adminEmail))
                {
                    return BadRequest(new { message = "Admin email is not configured" });
                }

                // Send the verification code via email
                await _emailService.SendVerificationCodeAsync(adminEmail, code);

                return Ok(new { message = "Verification code sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending verification code");
                return StatusCode(500, "An error occurred while sending verification code");
            }
        }

        /// <summary>
        /// Verifies the application using the provided code
        /// </summary>
        [HttpPost("verify")]
        public async Task<IActionResult> VerifyApplication([FromBody] VerificationRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Code))
                {
                    return BadRequest(new { message = "Verification code is required" });
                }

                bool isValid = await _verificationService.VerifyCodeAsync(request.Code);

                if (isValid)
                {
                    return Ok(new { message = "Application verified successfully" });
                }
                else
                {
                    return BadRequest(new { message = "Invalid verification code or code expired" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying application");
                return StatusCode(500, "An error occurred during verification");
            }
        }

        /// <summary>
        /// Resets the verification status (requires authentication in production)
        /// </summary>
        [HttpPost("reset")]
        public async Task<IActionResult> ResetVerification()
        {
            try
            {
                // In production, this endpoint should be protected with admin authentication
                bool success = await _verificationService.ResetVerificationAsync();
                if (success)
                {
                    return Ok(new { message = "Verification status reset successfully" });
                }
                else
                {
                    return StatusCode(500, "Failed to reset verification status");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting verification");
                return StatusCode(500, "An error occurred while resetting verification status");
            }
        }
    }

    public class VerificationRequest
    {
        public string Code { get; set; }
    }
}
