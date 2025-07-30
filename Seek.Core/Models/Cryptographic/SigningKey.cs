using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Models.Cryptographic
{
    public class SigningKey
    {
        // Key Identifiers (ZATCA Security Requirements)
        public string KeyId { get; set; } = Guid.NewGuid().ToString();
        public string PublicKey { get; set; }  // Base64-encoded
        public string? PrivateKeyReference { get; set; }  // HSM/KeyVault pointer

        // Metadata (ZATCA KSA-15)
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;
        public DateTime ExpiryDate { get; set; } = DateTime.UtcNow.AddYears(1);
        public string Issuer { get; set; } = "Seek.ZATCA";
        public string Algorithm { get; set; } = "RSA-2048";

        // Compliance Info
        public string? CertificateSerial { get; set; }  // From ZATCA
        public string? CertificatePEM { get; set; }  // Full cert chain

        /// <summary>
        /// Checks if key is valid for signing
        /// </summary>
        public bool IsActive()
        {
            return DateTime.UtcNow >= IssueDate &&
                   DateTime.UtcNow <= ExpiryDate &&
                   !string.IsNullOrEmpty(PublicKey);
        }
    }
}
