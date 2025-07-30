using Seek.ZATCA.Core.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.ZATCA.Core.Contracts.Responses.Compliance
{
    /// <summary>
    /// Response DTO for invoice signing operations (Phase 2)
    /// Reference: ZATCA E-Invoicing Implementation Standard vF, Section 7.2
    /// </summary>
    public class SigningResponse
    {
        /// <summary>
        /// Overall signing status
        /// </summary>
        public SigningStatus Status { get; set; }

        /// <summary>
        /// Signed invoice in UBL 2.1 format with digital signature
        /// </summary>
        public string? SignedInvoiceXml { get; set; }

        /// <summary>
        /// Cryptographic stamp (Base64-encoded)
        /// </summary>
        public string? CryptographicStamp { get; set; }

        /// <summary>
        /// Invoice hash (SHA-256) used for signing
        /// </summary>
        public string? InvoiceHash { get; set; }

        /// <summary>
        /// Technical details about the signing process
        /// </summary>
        public SigningTechnicalDetails TechnicalDetails { get; set; } = new();

        /// <summary>
        /// Timestamp of signing operation
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Any warnings or informational messages
        /// </summary>
        public List<SigningMessage> Messages { get; set; } = new();
    }
}
