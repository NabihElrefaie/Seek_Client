using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Seek.Core.Helper_Classes;
using Seek.Core.IRepositories;

namespace Seek.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatabaseSecurityController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly IRepo_Database_Security _securityRepo;

        public DatabaseSecurityController(IWebHostEnvironment env, IRepo_Database_Security securityRepo)
        {
            _env = env;
            _securityRepo = securityRepo;
        }

        [HttpPost("transform")]
        public async Task<IActionResult> TransformDatabase([FromBody] TransformRequest request)
        {
            var dbPath = Path.Combine(_env.ContentRootPath, "Database", "seek.db");
            var tempPath = Path.Combine(_env.ContentRootPath, "Database", "temp_transform.db");

            bool success;
            if (request.Is_Encrypt)
                success = await _securityRepo.EncryptDatabaseAsync(dbPath, tempPath, request.Encryption_Key);
            else
                success = await _securityRepo.DecryptDatabaseAsync(dbPath, tempPath, request.Encryption_Key);

            if (success)
                return Ok(request.Is_Encrypt ? "Encryption done." : "Decryption done.");
            else
                return StatusCode(500, "Operation failed.");
        }
    }
}
