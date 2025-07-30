using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Models.Cryptographic
{
    public class CryptographicStamp
    {
        public string Stamp { get; set; } // Base64
        public string Algorithm { get; set; } = "SHA256withRSA";
        public string PublicKey { get; set; } // Base64-encoded
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public TimeSpan ValidityDuration { get; set; } = TimeSpan.FromHours(24);

        // For key rotation
        public string KeyVersion { get; set; } = "1.0";
    }
}
