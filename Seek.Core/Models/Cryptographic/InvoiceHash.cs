using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Models.Cryptographic
{
    public class InvoiceHash
    {
        // Required by ZATCA (BR-32)
        public string Value { get; set; }  // Base64-encoded SHA-256 hash
        public string Algorithm { get; set; } = "SHA-256";
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        // For hash chaining (BR-33)
        public string? PreviousInvoiceHash { get; set; }
        public int HashChainIndex { get; set; } = 0;

        // Technical metadata
        public string InputPayload { get; set; }  // Original XML before hashing
        public string? Salt { get; set; }  // Optional for additional security

        /// <summary>
        /// Validates against ZATCA hashing rules
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Value)
                   && Value.Length == 44 // Base64 SHA-256 length
                   && (HashChainIndex == 0 || !string.IsNullOrEmpty(PreviousInvoiceHash));
        }
    }
}
