using javax.xml.crypto.dsig;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Seek.Core.Helper_Classes;
using Seek.Core.IRepositories;
using Serilog;
using static com.sun.net.httpserver.Authenticator;

namespace Seek.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatabaseSecurityController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly IRepo_Database_Security _securityRepo;
        private readonly IConfiguration _configuration;

        public DatabaseSecurityController(IWebHostEnvironment env, IRepo_Database_Security securityRepo, IConfiguration configuration)
        {
            _env = env;
            _securityRepo = securityRepo;
            _configuration = configuration;

        }

        [HttpPost("Decrypt")]
        public async Task<IActionResult> Decrypt([FromBody] TransformRequest request)
        {
            // Check if encryption key is needed (only if is_Encrypt is false)
            if (string.IsNullOrEmpty(request.Encryption_Key))
            {
                return BadRequest(new { message = "Encryption key is required when decrypting." });
            }
            var dbPath = Path.Combine(_env.ContentRootPath, "Database", "seek.db");
            var tempPath = Path.Combine(_env.ContentRootPath, "Database", "temp_transform.db");

            // Fetch the encryption key from the configuration (for encryption) or from the request (for decryption)
            var encryptionKey = request.Encryption_Key;
            if (string.IsNullOrEmpty(encryptionKey))
            {
                return StatusCode(500, new { message = "Encryption key is not set." });
            }
            var (success, Message) = await _securityRepo.DecryptDatabaseAsync(dbPath, tempPath, encryptionKey);
            return success ? Ok(new { message =Message }) : BadRequest(new { message = Message });

        }
        [HttpPost("Encrypt")]
        public async Task<IActionResult> Encrypt([FromQuery] string databasePath)
        {
            var tempPath = Path.Combine(_env.ContentRootPath, "Database", "temp_transform.db");

            // Fetch encryption key from configuration
            var encryptionKey = _configuration["EncryptionKey"];

            // Log for debugging
            Log.Information($"Encrypting database at: {databasePath}");

            if (string.IsNullOrEmpty(encryptionKey))
            {
                return StatusCode(500, new { message = "Encryption key is not set." });
            }

            if (!databasePath.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "The database file must have a .db extension." });
            }

            var (success, message) = await _securityRepo.EncryptDatabaseAsync(databasePath, tempPath, encryptionKey);

            // Log success or failure
            Log.Information(success ? "Encryption successful." : $"Encryption failed: {message}");

            return success ? Ok(new { message }) : BadRequest(new { message });
        }

    }
}
